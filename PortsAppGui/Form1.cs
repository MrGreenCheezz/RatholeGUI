using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace PortsAppGui
{
    public partial class Form1 : Form
    {
        private Process? process;
        private JsonDataClass _dataObject = new();
        private SshConnector? _clientConnector;
        private SshConnector? _serverConnector;
        private readonly Timer _statusTimer;
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _trayMenu;

        private readonly Image SuccessImage = Image.FromFile("./Resources/success.png");
        private readonly Image ErrorImage = Image.FromFile("./Resources/error.png");
        private bool _isFreshStart;
        private bool _clientBinaryOk;
        private bool _serverBinaryOk;
        private bool _clientSshFailed;
        private bool _serverSshFailed;
        private bool _isStatusCheckRunning;
        private bool _uiLoaded;
        private bool _isClosingFromTray;

        public Form1()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            Resize += Form1_Resize;

            _statusTimer = new Timer { Interval = 3000 };
            _statusTimer.Tick += async (_, _) => await RefreshStatusAsync();

            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Open", null, (_, _) => ShowFromTray());
            _trayMenu.Items.Add("Run", null, (_, _) => RunButton.PerformClick());
            _trayMenu.Items.Add("Stop", null, (_, _) => StopButton.PerformClick());
            _trayMenu.Items.Add("Exit", null, (_, _) =>
            {
                _isClosingFromTray = true;
                Close();
            });

            _trayIcon = new NotifyIcon
            {
                Text = "RatholeGUI",
                Icon = Icon,
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            _trayIcon.DoubleClick += (_, _) => ShowFromTray();
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            TryStartUbuntu();
            _dataObject = LoadJsonFromDataFile();
            LoadUiFromData();
            _uiLoaded = true;

            RebuildConnectors();
            if (_clientConnector == null || _serverConnector == null)
            {
                StatusLabel.Text = "Status: open settings and fill host:port";
                RunButton.Enabled = false;
                StopButton.Enabled = false;
                _statusTimer.Start();
                return;
            }

            StatusLabel.Text = "Status: checking rathole...";
            RunButton.Enabled = false;
            RestoreConnectionState();
            _statusTimer.Start();
            await RefreshStatusAsync();
        }
        private void TryStartUbuntu()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ubuntu.exe",
                    Arguments = "",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process = Process.Start(startInfo);
            }
            catch
            {
                process = null;
            }
        }

        private void LoadUiFromData()
        {
            panel1.Controls.Clear();

            if (_isFreshStart || string.IsNullOrEmpty(_dataObject.Configs.ServerAddress) || string.IsNullOrEmpty(_dataObject.Configs.ClientAddress))
            {
                ErrorText.Visible = true;
                panel1.Controls.Add(ErrorText);
            }

            for (int i = 0; i < _dataObject.Services.Count; i++)
            {
                AddServiceControl(_dataObject.Services[i], i);
            }

            ApplyServiceValidationStates();
        }

        private void AddServiceControl(Service service, int index)
        {
            var serviceControl = new ServiceControl();
            serviceControl.SetupControl(service);
            serviceControl.SetIndex(index);
            serviceControl.ExitClicked += ControlDeleted;
            serviceControl.ValueChanged += (_, _) => AutoSaveData();
            panel1.Controls.Add(serviceControl);
        }

        private void RebuildConnectors()
        {
            _clientConnector = null;
            _serverConnector = null;
            _clientBinaryOk = false;
            _serverBinaryOk = false;
            _clientSshFailed = false;
            _serverSshFailed = false;

            if (!ConfigValidator.TryParseHostPort(_dataObject.Configs.ClientAddress, out var clientHost, out var clientPort) ||
                !ConfigValidator.TryParseHostPort(_dataObject.Configs.ServerAddress, out var serverHost, out var serverPort))
            {
                return;
            }

            _clientConnector = new SshConnector(clientHost, clientPort, _dataObject.Configs.ClientUsername,
                _dataObject.Configs.ClientPassword);
            _serverConnector = new SshConnector(serverHost, serverPort, _dataObject.Configs.ServerUsername,
                _dataObject.Configs.ServerPassword);
        }

        private async Task RefreshStatusAsync()
        {
            if (_isStatusCheckRunning)
                return;

            _isStatusCheckRunning = true;
            try
            {
                if (!_clientBinaryOk || !_serverBinaryOk || _clientSshFailed || _serverSshFailed)
                    await Task.Run(CheckRatholeBinaries);

                if (!IsDisposed && !Disposing)
                    RefreshStatus();
            }
            finally
            {
                _isStatusCheckRunning = false;
            }
        }

        private void CheckRatholeBinaries()
        {
            if (_clientConnector == null || _serverConnector == null)
            {
                _clientSshFailed = true;
                _serverSshFailed = true;
                _clientBinaryOk = false;
                _serverBinaryOk = false;
                return;
            }

            var clientResult = _clientConnector.RatholeBinaryExists(_dataObject.Configs.ClientRatholePath);
            var serverResult = _serverConnector.RatholeBinaryExists(_dataObject.Configs.ServerRatholePath);

            _clientSshFailed = clientResult == null;
            _serverSshFailed = serverResult == null;
            _clientBinaryOk = clientResult == true;
            _serverBinaryOk = serverResult == true;
        }

        private void ApplyBinaryStatus()
        {
            if (_clientSshFailed || _serverSshFailed)
            {
                StatusLabel.Text = "Status: cannot check rathole (SSH error); retrying...";
            }
            else if (!_clientBinaryOk && !_serverBinaryOk)
            {
                StatusLabel.Text = "Status: rathole not found (client, server)";
            }
            else if (!_clientBinaryOk)
            {
                StatusLabel.Text = "Status: rathole not found (client)";
            }
            else if (!_serverBinaryOk)
            {
                StatusLabel.Text = "Status: rathole not found (server)";
            }
            else
            {
                return;
            }

            pictureBox1.Image = ErrorImage;
            RunButton.Enabled = false;
            StopButton.Enabled = false;
        }

        public JsonDataClass LoadJsonFromDataFile()
        {
            if (!File.Exists(Program.DataFilePath))
            {
                using var tmpFile = File.Create(Program.DataFilePath);
                _dataObject = new JsonDataClass();
                SaveData();
                _isFreshStart = true;
                return _dataObject;
            }

            try
            {
                string jsonFromFile = File.ReadAllText(Program.DataFilePath);
                var data = JsonSerializer.Deserialize<JsonDataClass>(jsonFromFile) ?? new JsonDataClass();
                data.Services ??= new List<Service>();
                data.Configs ??= new ConfigStore();
                return data;
            }
            catch (Exception)
            {
                throw new FileLoadException("No file");
            }
        }

        public void ControlDeleted(int index, ServiceControl service)
        {
            if (MessageBox.Show($"Delete service '{service.GetServiceData().ServiceName}'?", "Delete service", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            panel1.SuspendLayout();
            _dataObject.Services.RemoveAt(index);
            panel1.Controls.Remove(service);
            RearrangeElements();
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
            AutoSaveData();
        }

        public void AddControl()
        {
            panel1.SuspendLayout();
            var service = new Service();
            _dataObject.Services.Add(service);
            AddServiceControl(service, _dataObject.Services.Count - 1);
            RearrangeElements();
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
            AutoSaveData();
        }

        private void RearrangeElements()
        {
            var controlsArray = panel1.Controls.OfType<ServiceControl>().ToList();
            for (int i = 0; i < controlsArray.Count; i++)
            {
                controlsArray[i].Index = i;
            }
        }

        public void SaveData()
        {
            _dataObject.Services = GetCurrentServices();

            string dataToWrite = JsonSerializer.Serialize(_dataObject, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Program.DataFilePath, dataToWrite);
        }

        private void AutoSaveData()
        {
            if (!_uiLoaded)
                return;

            SaveData();
            ApplyServiceValidationStates();
        }

        private List<Service> GetCurrentServices()
        {
            return panel1.Controls
                .OfType<ServiceControl>()
                .OrderBy(control => control.Index)
                .Select(control => control.GetServiceData())
                .ToList();
        }

        private void AddRuleButton_Click(object sender, EventArgs e)
        {
            AddControl();
        }

        private void SaveRulesButton_Click(object sender, EventArgs e)
        {
            SaveData();
            MessageBox.Show("Configuration saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private void TestSshButton_Click(object sender, EventArgs e)
        {
            TestSshConnections();
        }

        private void PreviewTomlButton_Click(object sender, EventArgs e)
        {
            PreviewConfigs();
        }

        private void LogsButton_Click(object sender, EventArgs e)
        {
            OpenLogs();
        }

        public void WriteRulesToFile()
        {
            string clientFilePath = _dataObject.Configs.ClientTomlPath;
            string serverFilePath = _dataObject.Configs.ServerTomlPath;
            var services = GetCurrentServices();

            File.WriteAllText(clientFilePath, TomlGenerator.GenerateClientConfig(_dataObject.Configs, services), Encoding.UTF8);
            File.WriteAllText(serverFilePath, TomlGenerator.GenerateServerConfig(services), Encoding.UTF8);
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveData();
                var errors = ConfigValidator.Validate(_dataObject.Configs, GetCurrentServices());
                if (errors.Count > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errors), "Validation errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApplyServiceValidationStates();
                    return;
                }

                RebuildConnectors();
                CheckRatholeBinaries();
                if (!_clientBinaryOk || !_serverBinaryOk || _clientSshFailed || _serverSshFailed || _clientConnector == null || _serverConnector == null)
                {
                    ApplyBinaryStatus();
                    return;
                }

                WriteRulesToFile();

                var serverRemoteConfigPath = CombineRemotePath(_dataObject.Configs.ServerRatholePath, _dataObject.Configs.ServerTomlPath);
                var clientRemoteConfigPath = CombineRemotePath(_dataObject.Configs.ClientRatholePath, _dataObject.Configs.ClientTomlPath);

                _serverConnector.SendFile(_dataObject.Configs.ServerTomlPath, serverRemoteConfigPath);
                _serverConnector.BeginRatholeConnection(serverRemoteConfigPath, _dataObject.Configs.ServerRatholePath);
                _clientConnector.SendFile(_dataObject.Configs.ClientTomlPath, clientRemoteConfigPath);
                _clientConnector.BeginRatholeConnection(clientRemoteConfigPath, _dataObject.Configs.ClientRatholePath);
                SaveConnectionState();

                RefreshStatus();
            }
            catch (Exception ex)
            {
                var stopErrors = StopRatholeEverywhere();
                StatusLabel.Text = "Status: error";
                pictureBox1.Image = ErrorImage;
                RunButton.Enabled = stopErrors.Count == 0;
                StopButton.Enabled = stopErrors.Count > 0;
                var message = stopErrors.Count == 0
                    ? ex.Message
                    : $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Cleanup errors:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}";
                MessageBox.Show(message, "SSH error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isClosingFromTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _trayIcon.ShowBalloonTip(1500, "RatholeGUI", "Application minimized to tray.", ToolTipIcon.Info);
                return;
            }

            _statusTimer.Stop();
            var stopErrors = StopRatholeEverywhere();
            if (stopErrors.Count > 0 && e.CloseReason == CloseReason.UserClosing &&
                MessageBox.Show(
                    $"Could not stop every rathole process:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}{Environment.NewLine}{Environment.NewLine}Exit anyway?",
                    "Rathole cleanup failed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                e.Cancel = true;
                _isClosingFromTray = false;
                _statusTimer.Start();
                return;
            }

            if (process is { HasExited: false })
                process.Kill(true);
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            var stopErrors = StopRatholeEverywhere();
            if (stopErrors.Count > 0)
            {
                StatusLabel.Text = "Status: failed to stop every rathole process";
                pictureBox1.Image = ErrorImage;
                RunButton.Enabled = false;
                StopButton.Enabled = true;
                MessageBox.Show(string.Join(Environment.NewLine, stopErrors), "Rathole cleanup failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RefreshStatus();
        }

        private List<string> StopRatholeEverywhere()
        {
            var errors = new List<string>();
            var hasSavedConnection = ConnectionState.Load() != null;
            Stop("Client", _clientConnector);
            Stop("Server", _serverConnector);

            if (errors.Count == 0)
                ConnectionState.Clear();
            else
            {
                _clientSshFailed = true;
                _serverSshFailed = true;
                try
                {
                    SaveConnectionState();
                }
                catch (Exception ex)
                {
                    errors.Add($"State: {ex.Message}");
                }
            }

            return errors;

            void Stop(string name, SshConnector? connector)
            {
                if (connector == null)
                {
                    if (hasSavedConnection)
                        errors.Add($"{name}: SSH settings are invalid; saved rathole processes could not be reached.");
                    return;
                }

                try
                {
                    connector.StopAllRatholeProcesses();
                }
                catch (Exception ex)
                {
                    errors.Add($"{name}: {ex.Message}");
                }
            }
        }

        private static string CombineRemotePath(string remoteDir, string localPath)
        {
            var normalizedLocalPath = localPath.Replace('\\', '/');
            var fileName = normalizedLocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? localPath;
            return remoteDir.TrimEnd('/') + "/" + fileName;
        }

        private void RefreshStatus()
        {
            if (_clientConnector == null || _serverConnector == null)
            {
                StatusLabel.Text = "Status: invalid host:port";
                RunButton.Enabled = false;
                StopButton.Enabled = false;
                pictureBox1.Image = ErrorImage;
                return;
            }

            if (!_clientBinaryOk || !_serverBinaryOk || _clientSshFailed || _serverSshFailed)
            {
                ApplyBinaryStatus();
                return;
            }

            bool clientUp = _clientConnector.IsRatholeRunning();
            bool serverUp = _serverConnector.IsRatholeRunning();
            bool running = clientUp && serverUp;
            bool anyRunning = clientUp || serverUp;

            StopButton.Enabled = anyRunning;
            RunButton.Enabled = !anyRunning;
            pictureBox1.Image = running ? SuccessImage : ErrorImage;
            StatusLabel.Text = running
                ? "Status: Server Running / Client Running"
                : $"Status: Server {(serverUp ? "Running" : "Stopped")} / Client {(clientUp ? "Running" : "Stopped")}";

            if (!anyRunning)
                ConnectionState.Clear();
        }

        private void OpenSettings()
        {
            using var form = new SettingsForm(_dataObject.Configs, () =>
            {
                SaveData();
                RebuildConnectors();
            });
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _isFreshStart = false;
                ErrorText.Visible = false;
                panel1.Controls.Remove(ErrorText);
                RebuildConnectors();
                RestoreConnectionState();
                RefreshStatus();
            }
        }

        private void RestoreConnectionState()
        {
            var state = ConnectionState.Load();
            if (state == null || !state.Matches(_dataObject.Configs) || _clientConnector == null || _serverConnector == null)
                return;

            _serverConnector.LoadProcessPid(state.ServerPid);
            _clientConnector.LoadProcessPid(state.ClientPid);
        }

        private void SaveConnectionState()
        {
            if (_serverConnector == null || _clientConnector == null)
                return;

            ConnectionState.Save(new ConnectionState
            {
                ServerAddress = _dataObject.Configs.ServerAddress,
                ClientAddress = _dataObject.Configs.ClientAddress,
                ServerPid = _serverConnector.ProcessPid,
                ClientPid = _clientConnector.ProcessPid
            });
        }

        private void TestSshConnections()
        {
            SaveData();
            RebuildConnectors();
            if (_serverConnector == null || _clientConnector == null)
            {
                MessageBox.Show("Проверь Server/Client address в формате host:port.", "Invalid settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var serverOk = _serverConnector.TestConnection(out var serverError);
            var clientOk = _clientConnector.TestConnection(out var clientError);
            MessageBox.Show(
                $"Server SSH: {(serverOk ? "OK" : serverError)}{Environment.NewLine}Client SSH: {(clientOk ? "OK" : clientError)}",
                "SSH test", MessageBoxButtons.OK, serverOk && clientOk ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void PreviewConfigs()
        {
            SaveData();
            var errors = ConfigValidator.Validate(_dataObject.Configs, GetCurrentServices());
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors), "Validation errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var form = new PreviewForm(
                TomlGenerator.GenerateClientConfig(_dataObject.Configs, GetCurrentServices()),
                TomlGenerator.GenerateServerConfig(GetCurrentServices()));
            form.ShowDialog(this);
        }

        private void OpenLogs()
        {
            RebuildConnectors();
            if (_serverConnector == null || _clientConnector == null)
            {
                MessageBox.Show("Проверь Server/Client address в формате host:port.", "Invalid settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var form = new LogViewerForm(
                () => _serverConnector.ReadRatholeLog(),
                () => _clientConnector.ReadRatholeLog());
            form.ShowDialog(this);
        }

        private void ApplyServiceValidationStates()
        {
            foreach (var serviceControl in panel1.Controls.OfType<ServiceControl>())
                serviceControl.ApplyValidationState();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }
    }

    public class JsonDataClass
    {
        public ConfigStore Configs { get; set; } = new();
        public List<Service> Services { get; set; } = new();
    }
}
