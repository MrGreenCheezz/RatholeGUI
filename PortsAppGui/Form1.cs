using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private readonly Timer _saveTimer;
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
        private bool _isBusy;
        private bool _shutdownReady;
        private bool _isExiting;
        private int _connectorGeneration;

        // How long rathole gets to read its config and bind ports before we check it is still alive.
        private static readonly TimeSpan StartupGracePeriod = TimeSpan.FromMilliseconds(1500);

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

            // Typing in a service card saves after a short pause instead of on every keystroke.
            _saveTimer = new Timer { Interval = 500 };
            _saveTimer.Tick += (_, _) => SaveData();

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
            _dataObject = LoadDataFile(out var brokenCopyPath);
            LoadUiFromData();
            _uiLoaded = true;

            if (brokenCopyPath != null)
                Dialogs.Error(this, "data.json could not be read",
                    $"The file was damaged and has been moved to {brokenCopyPath}.{Environment.NewLine}RatholeGUI started with an empty configuration.");

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

        private JsonDataClass LoadDataFile(out string? brokenCopyPath)
        {
            var data = DataFile.Load(Program.DataFilePath, out brokenCopyPath);
            if (data != null && brokenCopyPath == null)
                return data;

            _dataObject = data ?? new JsonDataClass();
            SaveData();
            _isFreshStart = true;
            return _dataObject;
        }

        public void SaveData()
        {
            _saveTimer.Stop();
            _dataObject.Services = GetCurrentServices();
            DataFile.Save(Program.DataFilePath, _dataObject);
        }

        private void AutoSaveData()
        {
            if (!_uiLoaded)
                return;

            ApplyServiceValidationStates();
            _saveTimer.Stop();
            _saveTimer.Start();
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
            File.WriteAllText(serverFilePath, TomlGenerator.GenerateServerConfig(_dataObject.Configs, services), Encoding.UTF8);
        }

        #endregion

        #region Connectors and status

        /// <summary>
        /// Points the connectors at the current settings. A connector whose SSH target did not change
        /// is kept, together with its open session and the PID it started.
        /// </summary>
        private void RebuildConnectors()
        {
            _connectorGeneration++;
            _clientBinaryOk = false;
            _serverBinaryOk = false;
            _clientSshFailed = false;
            _serverSshFailed = false;

            var config = _dataObject.Configs;
            _clientConnector = Reuse(_clientConnector, config.ClientAddress, config.ClientUsername, config.ClientPassword);
            _serverConnector = Reuse(_serverConnector, config.ServerAddress, config.ServerUsername, config.ServerPassword);

            static SshConnector? Reuse(SshConnector? current, string address, string username, string password)
            {
                if (ConfigValidator.TryParseHostPort(address, out var host, out var port) &&
                    current != null && current.HasSameTarget(host, port, username, password))
                    return current;

                current?.Dispose();
                return ConfigValidator.TryParseHostPort(address, out host, out port)
                    ? new SshConnector(host, port, username, password)
                    : null;
            }
        }

        private bool BinariesOk => _clientBinaryOk && _serverBinaryOk && !_clientSshFailed && !_serverSshFailed;

        /// <summary>What one round of SSH checks found. Null = that machine could not be reached.</summary>
        private sealed record StatusProbe(bool? ClientBinary, bool? ServerBinary, bool? ClientRunning, bool? ServerRunning);

        private async Task RefreshStatusAsync()
        {
            if (_isStatusCheckRunning || _isBusy)
                return;

            var client = _clientConnector;
            var server = _serverConnector;
            if (client == null || server == null)
            {
                SetStatus(StatusKind.Error, "Not configured", "Server/Client address must be in host:port format.");
                RunButton.Enabled = false;
                StopButton.Enabled = false;
                return;
            }

            _isStatusCheckRunning = true;
            try
            {
                var generation = _connectorGeneration;
                var checkBinaries = !BinariesOk;
                var clientDir = _dataObject.Configs.ClientRatholePath;
                var serverDir = _dataObject.Configs.ServerRatholePath;

                var probe = await Task.Run(() => ProbeStatus(client, server, checkBinaries, clientDir, serverDir));

                // Settings changed or Run/Stop started while we were waiting: this result is stale.
                if (IsDisposed || Disposing || _isBusy || generation != _connectorGeneration)
                    return;

                ApplyStatus(probe);
            }
            finally
            {
                _isStatusCheckRunning = false;
            }
        }

        private static StatusProbe ProbeStatus(SshConnector client, SshConnector server, bool checkBinaries,
            string clientDir, string serverDir)
        {
            if (checkBinaries)
            {
                var clientBinary = client.RatholeBinaryExists(clientDir);
                var serverBinary = server.RatholeBinaryExists(serverDir);
                if (clientBinary != true || serverBinary != true)
                    return new StatusProbe(clientBinary, serverBinary, null, null);
            }

            return new StatusProbe(true, true, client.IsRatholeRunning(), server.IsRatholeRunning());
        }

        private void ApplyStatus(StatusProbe probe)
        {
            _clientBinaryOk = probe.ClientBinary == true;
            _serverBinaryOk = probe.ServerBinary == true;
            _clientSshFailed = probe.ClientBinary == null || (_clientBinaryOk && _serverBinaryOk && probe.ClientRunning == null);
            _serverSshFailed = probe.ServerBinary == null || (_clientBinaryOk && _serverBinaryOk && probe.ServerRunning == null);

            if (!BinariesOk)
            {
                ApplyBinaryStatus();
                return;
            }

            ApplyRunningStatus(probe.ClientRunning == true, probe.ServerRunning == true);
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

        private void ApplyRunningStatus(bool clientUp, bool serverUp)
        {
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

        /// <summary>
        /// Locks Run/Stop, the status poll and everything that could swap the SSH connectors while a
        /// long SSH operation runs in the background.
        /// </summary>
        private void BeginBusy(string pillText, string detail)
        {
            _isBusy = true;
            SetStatus(StatusKind.Busy, pillText, detail);
            RunButton.Enabled = false;
            StopButton.Enabled = false;
            SetConnectionCommandsEnabled(false);
        }

        private void EndBusy()
        {
            _isBusy = false;
            SetConnectionCommandsEnabled(true);
        }

        private void SetConnectionCommandsEnabled(bool enabled)
        {
            SettingsButton.Enabled = enabled;
            HintButton.Enabled = enabled;
            TestSshButton.Enabled = enabled;
            LogsButton.Enabled = enabled;
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

            // rathole's own port is busy on the server too.
            if (int.TryParse(_dataObject.Configs.ControlPort, out var controlPort))
                taken.Add(controlPort);

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

        private async void TestSshButton_Click(object? sender, EventArgs e)
        {
            await TestSshConnectionsAsync();
        }

        private void PreviewTomlButton_Click(object? sender, EventArgs e)
        {
            PreviewConfigs();
        }

        private void LogsButton_Click(object? sender, EventArgs e)
        {
            OpenLogs();
        }

        private async void RunButton_Click(object? sender, EventArgs e)
        {
            if (_isBusy)
                return;

            SaveData();
            var errors = ConfigValidator.Validate(_dataObject.Configs, GetCurrentServices());
            if (errors.Count > 0)
            {
                Dialogs.Error(this, "Configuration is not valid", string.Join(Environment.NewLine, errors));
                ApplyServiceValidationStates();
                return;
            }

            RebuildConnectors();
            var client = _clientConnector;
            var server = _serverConnector;
            if (client == null || server == null)
                return; // Validate() already rejects addresses that are not host:port

            BeginBusy("Connecting…", "Uploading configs and starting rathole…");
            try
            {
                WriteRulesToFile();
                var config = _dataObject.Configs;
                var binaryProblem = await Task.Run(() => StartTunnel(client, server, config));
                EndBusy();

                if (binaryProblem != null)
                {
                    ApplyStatus(binaryProblem);
                    return;
                }

                SaveConnectionState();
                _clientBinaryOk = _serverBinaryOk = true;
                _clientSshFailed = _serverSshFailed = false;
                ApplyRunningStatus(clientUp: true, serverUp: true);
            }
            catch (Exception ex)
            {
                var stopErrors = await StopRatholeEverywhereAsync();
                EndBusy();
                SetStatus(StatusKind.Error, "Failed to start", FirstLine(ex.Message));
                RunButton.Enabled = stopErrors.Count == 0;
                StopButton.Enabled = stopErrors.Count > 0;
                var message = stopErrors.Count == 0
                    ? ex.Message
                    : $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Cleanup errors:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}";
                Dialogs.Error(this, "Failed to start rathole", message);
            }
            finally
            {
                EndBusy();
            }

            static string FirstLine(string text) => text.Split('\n', 2)[0].TrimEnd('\r');
        }

        /// <summary>
        /// Runs on a worker thread. Returns the probe when a rathole binary is missing or SSH is down
        /// (nothing was started); throws when uploading or starting fails.
        /// </summary>
        private static StatusProbe? StartTunnel(SshConnector client, SshConnector server, ConfigStore config)
        {
            var clientBinary = client.RatholeBinaryExists(config.ClientRatholePath);
            var serverBinary = server.RatholeBinaryExists(config.ServerRatholePath);
            if (clientBinary != true || serverBinary != true)
                return new StatusProbe(clientBinary, serverBinary, null, null);

            var serverRemoteConfigPath = CombineRemotePath(config.ServerRatholePath, config.ServerTomlPath);
            var clientRemoteConfigPath = CombineRemotePath(config.ClientRatholePath, config.ClientTomlPath);

            server.SendFile(config.ServerTomlPath, serverRemoteConfigPath);
            server.StartRathole(serverRemoteConfigPath, config.ServerRatholePath);
            client.SendFile(config.ClientTomlPath, clientRemoteConfigPath);
            client.StartRathole(clientRemoteConfigPath, config.ClientRatholePath);

            // A bad config or a port that is already taken makes rathole exit almost immediately;
            // report it now instead of showing "running" until the next poll.
            Thread.Sleep(StartupGracePeriod);
            server.EnsureStarted();
            client.EnsureStarted();
            return null;
        }

        private async void StopButton_Click(object? sender, EventArgs e)
        {
            if (_isBusy)
                return;

            BeginBusy("Stopping…", "Stopping rathole on both machines…");
            List<string> stopErrors;
            try
            {
                stopErrors = await StopRatholeEverywhereAsync();
            }
            finally
            {
                EndBusy();
            }

            if (stopErrors.Count > 0)
            {
                SetStatus(StatusKind.Error, "Cleanup failed", "Failed to stop every rathole process.");
                RunButton.Enabled = false;
                StopButton.Enabled = true;
                Dialogs.Error(this, "Rathole cleanup failed", string.Join(Environment.NewLine, stopErrors));
                return;
            }

            ApplyRunningStatus(clientUp: false, serverUp: false);
        }

        private async Task<List<string>> StopRatholeEverywhereAsync()
        {
            var client = _clientConnector;
            var server = _serverConnector;
            var hasSavedConnection = ConnectionState.Load() != null;
            var errors = await Task.Run(() => StopRathole(client, server, hasSavedConnection));
            return RecordStopResult(errors);
        }

        /// <summary>Blocking variant for when Windows is shutting down and there is no time to await.</summary>
        private List<string> StopRatholeEverywhere()
        {
            var errors = StopRathole(_clientConnector, _serverConnector, ConnectionState.Load() != null);
            return RecordStopResult(errors);
        }

        private List<string> RecordStopResult(List<string> errors)
        {
            if (errors.Count == 0)
            {
                ConnectionState.Clear();
                return errors;
            }

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

            return errors;
        }

        private static List<string> StopRathole(SshConnector? client, SshConnector? server, bool hasSavedConnection)
        {
            var errors = new List<string>();
            Stop("Client", client);
            Stop("Server", server);
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
                if (!_isBusy)
                    SetStatus(StatusKind.Busy, "Checking…", "Checking rathole on both machines…");
                _ = RefreshStatusAsync();
            }
        }

        private async Task TestSshConnectionsAsync()
        {
            SaveData();
            RebuildConnectors();
            var server = _serverConnector;
            var client = _clientConnector;
            if (server == null || client == null)
            {
                Dialogs.Error(this, "Invalid settings", "Server/Client address must be in host:port format.");
                return;
            }

            TestSshButton.Enabled = false;
            UseWaitCursor = true;
            string message;
            bool allOk;
            try
            {
                (message, allOk) = await Task.Run(() =>
                {
                    var serverOk = server.TestConnection(out var serverError);
                    var clientOk = client.TestConnection(out var clientError);
                    return ($"Server SSH: {(serverOk ? "OK" : serverError)}{Environment.NewLine}Client SSH: {(clientOk ? "OK" : clientError)}",
                        serverOk && clientOk);
                });
            }
            finally
            {
                UseWaitCursor = false;
                TestSshButton.Enabled = !_isBusy;
            }

            if (allOk)
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
                TomlGenerator.GenerateServerConfig(_dataObject.Configs, GetCurrentServices()));
            form.ShowDialog(this);
        }

        private void OpenLogs()
        {
            RebuildConnectors();
            var server = _serverConnector;
            var client = _clientConnector;
            if (server == null || client == null)
            {
                Dialogs.Error(this, "Invalid settings", "Server/Client address must be in host:port format.");
                return;
            }

            using var form = new LogViewerForm(
                () => server.ReadRatholeLog(),
                () => client.ReadRatholeLog());
            form.ShowDialog(this);
        }

        #endregion

        #region Window lifetime

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isClosingFromTray && !_shutdownReady && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _trayIcon.ShowBalloonTip(1500, "RatholeGUI", "Application minimized to tray.", ToolTipIcon.Info);
                return;
            }

            if (!_shutdownReady && e.CloseReason == CloseReason.UserClosing)
            {
                // Stopping rathole goes over SSH: do it without freezing the window, then close for real.
                e.Cancel = true;
                await PrepareShutdownAsync();
                return;
            }

            _statusTimer.Stop();
            if (!_shutdownReady)
            {
                // Windows is shutting down or the app is being terminated: no time for dialogs.
                try
                {
                    SaveData();
                }
                catch
                {
                    // Still stop rathole below even if the disk is unavailable.
                }

                if (!_dataObject.Configs.KeepTunnelOnExit)
                    StopRatholeEverywhere();
            }

            if (process is { HasExited: false })
                process.Kill(true);
            _clientConnector?.Dispose();
            _serverConnector?.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }

        private async Task PrepareShutdownAsync()
        {
            if (_isExiting)
                return; // Exit was clicked again while rathole is being stopped

            if (_isBusy)
            {
                // Run/Stop is still talking to the servers; let it finish first.
                _isClosingFromTray = false;
                Dialogs.Info(this, "RatholeGUI is busy", "Wait until the current operation finishes, then exit again.");
                return;
            }

            _isExiting = true;
            _statusTimer.Stop();
            SaveData();

            if (!_dataObject.Configs.KeepTunnelOnExit)
            {
                BeginBusy("Stopping…", "Stopping rathole before exit…");
                List<string> stopErrors;
                try
                {
                    stopErrors = await StopRatholeEverywhereAsync();
                }
                finally
                {
                    EndBusy();
                }

                if (stopErrors.Count > 0 &&
                    !Dialogs.Confirm(this, "Rathole cleanup failed",
                        $"Could not stop every rathole process:{Environment.NewLine}{string.Join(Environment.NewLine, stopErrors)}{Environment.NewLine}{Environment.NewLine}Exit anyway?",
                        "Exit anyway", "Stay open"))
                {
                    _isExiting = false;
                    _isClosingFromTray = false;
                    _statusTimer.Start();
                    _ = RefreshStatusAsync();
                    return;
                }
            }

            _shutdownReady = true;
            Close();
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
}
