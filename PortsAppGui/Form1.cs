using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortsAppGui.UI;
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

        private bool _isFreshStart;
        private bool _clientBinaryOk;
        private bool _serverBinaryOk;
        private bool _clientSshFailed;
        private bool _serverSshFailed;
        private bool _isStatusCheckRunning;
        private bool _uiLoaded;
        private bool _isClosingFromTray;
        private bool _isSyncingCardWidths;

        public Form1()
        {
            InitializeComponent();

            Icon = Theme.LoadAppIcon();
            LogoBox.Image = Icon.ToBitmap();
            Theme.ApplyTo(this);

            this.Load += MainForm_Load;
            Resize += Form1_Resize;
            HeaderPanel.Resize += (_, _) => PositionStatusPill();
            panel1.ClientSizeChanged += (_, _) => SyncCardWidths();

            _statusTimer = new Timer { Interval = 3000 };
            _statusTimer.Tick += async (_, _) => await RefreshStatusAsync();

            _trayMenu = new ContextMenuStrip
            {
                Renderer = new DarkMenuRenderer(),
                BackColor = Theme.Surface,
                ForeColor = Theme.Text,
                Font = Theme.Body
            };
            _trayMenu.Items.Add("Open", null, (_, _) => ShowFromTray());
            _trayMenu.Items.Add("Run", null, (_, _) => RunButton.PerformClick());
            _trayMenu.Items.Add("Stop", null, (_, _) => StopButton.PerformClick());
            _trayMenu.Items.Add(new ToolStripSeparator());
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
                SetStatus(StatusKind.Error, "Not configured", "Open Settings and fill in host:port for both machines.");
                RunButton.Enabled = false;
                StopButton.Enabled = false;
                _statusTimer.Start();
                return;
            }

            SetStatus(StatusKind.Busy, "Checking…", "Checking rathole on both machines…");
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

        #region Status presentation

        private void SetStatus(StatusKind kind, string pillText, string? detail = null)
        {
            ConnectionPill.Kind = kind;
            ConnectionPill.Text = pillText;
            PositionStatusPill();

            StatusLabel.Text = detail ?? pillText;
            StatusLabel.ForeColor = kind == StatusKind.Error ? Theme.Danger : Theme.TextMuted;
        }

        private void PositionStatusPill()
        {
            ConnectionPill.Left = Math.Max(TitleLabel.Right + 24, HeaderPanel.ClientSize.Width - 24 - ConnectionPill.Width);
            ConnectionPill.Top = (HeaderPanel.ClientSize.Height - ConnectionPill.Height) / 2;
        }

        #endregion

        #region Services list

        private void LoadUiFromData()
        {
            panel1.SuspendLayout();

            foreach (var control in panel1.Controls.OfType<ServiceControl>().ToList())
            {
                panel1.Controls.Remove(control);
                control.Dispose();
            }

            for (int i = 0; i < _dataObject.Services.Count; i++)
                AddServiceControl(_dataObject.Services[i], i);

            panel1.ResumeLayout(true);
            panel1.PerformLayout();

            UpdateServicesUi();
            ApplyServiceValidationStates();
        }

        private ServiceControl AddServiceControl(Service service, int index)
        {
            var serviceControl = new ServiceControl();
            serviceControl.SetupControl(service);
            serviceControl.SetIndex(index);
            serviceControl.ExitClicked += ControlDeleted;
            serviceControl.ValueChanged += (_, _) => AutoSaveData();
            panel1.Controls.Add(serviceControl);
            return serviceControl;
        }

        private void UpdateServicesUi()
        {
            var count = panel1.Controls.OfType<ServiceControl>().Count();

            ServicesCountLabel.Text = count == 0 ? "" : count == 1 ? "1 service" : $"{count} services";
            ServicesCountLabel.Left = ServicesLabel.Right + 12;
            EmptyStateLabel.Visible = count == 0;

            HintCard.Visible = _isFreshStart || !ConnectionSettingsLookComplete();

            SyncCardWidths();
        }

        private bool ConnectionSettingsLookComplete()
        {
            var config = _dataObject.Configs;
            return ConfigValidator.TryParseHostPort(config.ServerAddress, out _, out _) &&
                   ConfigValidator.TryParseHostPort(config.ClientAddress, out _, out _) &&
                   !string.IsNullOrWhiteSpace(config.ServerRatholePath) &&
                   !string.IsNullOrWhiteSpace(config.ClientRatholePath) &&
                   !string.IsNullOrWhiteSpace(config.ServerTomlPath) &&
                   !string.IsNullOrWhiteSpace(config.ClientTomlPath);
        }

        /// <summary>Cards fill the width of the scroll area, so the layout breathes when the window grows.</summary>
        private void SyncCardWidths()
        {
            if (_isSyncingCardWidths)
                return;

            _isSyncingCardWidths = true;
            try
            {
                // Resizing the cards can add or remove the scrollbar, which changes the width they
                // should have had; a second pass settles it.
                for (var pass = 0; pass < 2; pass++)
                {
                    var available = panel1.ClientSize.Width - panel1.Padding.Horizontal;
                    if (available <= 0)
                        return;

                    var changed = false;
                    foreach (Control control in panel1.Controls)
                    {
                        if (control is not CardPanel && control != EmptyStateLabel)
                            continue;

                        var width = Math.Max(control.MinimumSize.Width, available - control.Margin.Horizontal);
                        if (control.Width == width)
                            continue;

                        control.Width = width;
                        changed = true;
                    }

                    if (!changed)
                        break;
                }
            }
            finally
            {
                _isSyncingCardWidths = false;
            }
        }

        public void ControlDeleted(int index, ServiceControl service)
        {
            if (!Dialogs.ConfirmDanger(this, "Delete service",
                    $"Remove '{DisplayName(service)}' from the configuration?"))
                return;

            panel1.SuspendLayout();
            _dataObject.Services.RemoveAt(index);
            panel1.Controls.Remove(service);
            service.Dispose();
            RearrangeElements();
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
            AutoSaveData();
            UpdateServicesUi();

            static string DisplayName(ServiceControl control)
            {
                var name = control.GetServiceData().ServiceName;
                return string.IsNullOrWhiteSpace(name) ? "this service" : name;
            }
        }

        public void AddControl(Service? service = null)
        {
            panel1.SuspendLayout();
            service ??= new Service();
            _dataObject.Services.Add(service);
            var control = AddServiceControl(service, _dataObject.Services.Count - 1);
            RearrangeElements();
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
            AutoSaveData();
            UpdateServicesUi();
            panel1.ScrollControlIntoView(control);
        }

        private void RearrangeElements()
        {
            var controlsArray = panel1.Controls.OfType<ServiceControl>().ToList();
            for (int i = 0; i < controlsArray.Count; i++)
            {
                controlsArray[i].Index = i;
            }
        }

        private void ApplyServiceValidationStates()
        {
            foreach (var serviceControl in panel1.Controls.OfType<ServiceControl>())
                serviceControl.ApplyValidationState();
        }

        #endregion

        #region Data

        public JsonDataClass LoadJsonFromDataFile()
        {
            if (!File.Exists(Program.DataFilePath))
            {
                // SaveData creates the file itself; opening it here first would keep the
                // handle locked for the whole method and make the write fail.
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

        public void WriteRulesToFile()
        {
            string clientFilePath = _dataObject.Configs.ClientTomlPath;
            string serverFilePath = _dataObject.Configs.ServerTomlPath;
            var services = GetCurrentServices();

            File.WriteAllText(clientFilePath, TomlGenerator.GenerateClientConfig(_dataObject.Configs, services), Encoding.UTF8);
            File.WriteAllText(serverFilePath, TomlGenerator.GenerateServerConfig(services), Encoding.UTF8);
        }

        #endregion

        #region Connectors and status

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
                SetStatus(StatusKind.Error, "SSH unreachable", "Cannot check rathole over SSH; retrying every few seconds…");
            }
            else if (!_clientBinaryOk && !_serverBinaryOk)
            {
                SetStatus(StatusKind.Error, "rathole missing", "rathole binary not found on the client and the server.");
            }
            else if (!_clientBinaryOk)
            {
                SetStatus(StatusKind.Error, "rathole missing", "rathole binary not found on the client machine.");
            }
            else if (!_serverBinaryOk)
            {
                SetStatus(StatusKind.Error, "rathole missing", "rathole binary not found on the server machine.");
            }
            else
            {
                return;
            }

            RunButton.Enabled = false;
            StopButton.Enabled = false;
        }

        private void RefreshStatus()
        {
            if (_clientConnector == null || _serverConnector == null)
            {
                SetStatus(StatusKind.Error, "Not configured", "Server/Client address must be in host:port format.");
                RunButton.Enabled = false;
                StopButton.Enabled = false;
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

            if (running)
                SetStatus(StatusKind.Running, "Tunnel running", "Server running / Client running");
            else if (anyRunning)
                SetStatus(StatusKind.Busy, "Partially running",
                    $"Server {(serverUp ? "running" : "stopped")} / Client {(clientUp ? "running" : "stopped")}");
            else
                SetStatus(StatusKind.Stopped, "Stopped", "Server stopped / Client stopped");

            if (!anyRunning)
                ConnectionState.Clear();
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

        #endregion

        #region Commands

        private void AddRuleButton_Click(object? sender, EventArgs e)
        {
            AddControl();
        }

        private void AddFromAppButton_Click(object? sender, EventArgs e)
        {
            using var picker = new PortPickerForm();
            if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedEndpoint == null)
                return;

            var endpoint = picker.SelectedEndpoint;
            AddControl(new Service
            {
                ServiceName = MakeUniqueServiceName(ServiceControl.SuggestServiceName(endpoint)),
                ServiceToken = TokenGenerator.Create(),
                ClientAddress = endpoint.SuggestedLocalAddress,
                ClientPort = endpoint.Port.ToString(),
                ServerAddress = "0.0.0.0",
                ServerPort = SuggestFreeServerPort(endpoint.Port).ToString(),
                NoDelay = true,
                UdpEnabled = endpoint.Protocol == "UDP",
                Enabled = true
            });
        }

        private string MakeUniqueServiceName(string candidate)
        {
            var taken = new HashSet<string>(
                GetCurrentServices().Select(service => service.ServiceName),
                StringComparer.OrdinalIgnoreCase);

            if (!taken.Contains(candidate))
                return candidate;

            for (var suffix = 2; suffix < 100; suffix++)
            {
                var name = $"{candidate}-{suffix}";
                if (!taken.Contains(name))
                    return name;
            }

            return candidate;
        }

        private int SuggestFreeServerPort(int preferred)
        {
            var taken = GetCurrentServices()
                .Select(service => int.TryParse(service.ServerPort, out var port) ? port : -1)
                .Where(port => port > 0)
                .ToHashSet();

            var candidate = preferred;
            while (candidate <= 65535 && taken.Contains(candidate))
                candidate++;

            return candidate > 65535 ? preferred : candidate;
        }

        private void SaveRulesButton_Click(object? sender, EventArgs e)
        {
            SaveData();
            Dialogs.Success(this, "Configuration saved", $"Services were written to {Program.DataFilePath}.");
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            OpenSettings();
        }

        private void TestSshButton_Click(object? sender, EventArgs e)
        {
            TestSshConnections();
        }

        private void PreviewTomlButton_Click(object? sender, EventArgs e)
        {
            PreviewConfigs();
        }

        private void LogsButton_Click(object? sender, EventArgs e)
        {
            OpenLogs();
        }

        private void RunButton_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveData();
                var errors = ConfigValidator.Validate(_dataObject.Configs, GetCurrentServices());
                if (errors.Count > 0)
                {
                    Dialogs.Error(this, "Configuration is not valid", string.Join(Environment.NewLine, errors));
                    ApplyServiceValidationStates();
                    return;
                }

                RebuildConnectors();
                SetStatus(StatusKind.Busy, "Connecting…", "Uploading configs and starting rathole…");
                RunButton.Enabled = false;
                StopButton.Enabled = false;
                Update(); // the SSH calls below block the UI thread; show the new status first

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
                SetStatus(StatusKind.Error, "Failed to start", ex.Message);
                RunButton.Enabled = stopErrors.Count == 0;
                StopButton.Enabled = stopErrors.Count > 0;
                var message = stopErrors.Count == 0
                    ? ex.Message
                    : $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Cleanup errors:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}";
                Dialogs.Error(this, "SSH error", message);
            }
        }

        private void StopButton_Click(object? sender, EventArgs e)
        {
            var stopErrors = StopRatholeEverywhere();
            if (stopErrors.Count > 0)
            {
                SetStatus(StatusKind.Error, "Cleanup failed", "Failed to stop every rathole process.");
                RunButton.Enabled = false;
                StopButton.Enabled = true;
                Dialogs.Error(this, "Rathole cleanup failed", string.Join(Environment.NewLine, stopErrors));
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
                RebuildConnectors();
                RestoreConnectionState();
                UpdateServicesUi();
                RefreshStatus();
            }
        }

        private void TestSshConnections()
        {
            SaveData();
            RebuildConnectors();
            if (_serverConnector == null || _clientConnector == null)
            {
                Dialogs.Error(this, "Invalid settings", "Server/Client address must be in host:port format.");
                return;
            }

            var serverOk = _serverConnector.TestConnection(out var serverError);
            var clientOk = _clientConnector.TestConnection(out var clientError);
            var message = $"Server SSH: {(serverOk ? "OK" : serverError)}{Environment.NewLine}Client SSH: {(clientOk ? "OK" : clientError)}";

            if (serverOk && clientOk)
                Dialogs.Success(this, "SSH test", message);
            else
                Dialogs.Error(this, "SSH test", message);
        }

        private void PreviewConfigs()
        {
            SaveData();
            var errors = ConfigValidator.Validate(_dataObject.Configs, GetCurrentServices());
            if (errors.Count > 0)
            {
                Dialogs.Error(this, "Configuration is not valid", string.Join(Environment.NewLine, errors));
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
                Dialogs.Error(this, "Invalid settings", "Server/Client address must be in host:port format.");
                return;
            }

            using var form = new LogViewerForm(
                () => _serverConnector.ReadRatholeLog(),
                () => _clientConnector.ReadRatholeLog());
            form.ShowDialog(this);
        }

        #endregion

        #region Window lifetime

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
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
                !Dialogs.Confirm(this, "Rathole cleanup failed",
                    $"Could not stop every rathole process:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}{Environment.NewLine}{Environment.NewLine}Exit anyway?",
                    "Exit anyway", "Stay open"))
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

        #endregion
    }

    public class JsonDataClass
    {
        public ConfigStore Configs { get; set; } = new();
        public List<Service> Services { get; set; } = new();
    }
}
