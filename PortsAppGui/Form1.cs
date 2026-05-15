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
        private Process process;
        private JsonDataClass _dataObject;
        private SshConnector _clientConnector;
        private SshConnector _serverConnector;
        private readonly Timer _statusTimer;

        private Image SuccessImage = Image.FromFile("./Resources/success.png");
        private Image ErrorImage = Image.FromFile("./Resources/error.png");
        private bool _isFreshStart = false;
        private bool _clientBinaryOk;
        private bool _serverBinaryOk;
        private bool _clientSshFailed;
        private bool _serverSshFailed;

        public Form1()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            _statusTimer = new Timer { Interval = 3000 };
            _statusTimer.Tick += (s, e) => RefreshStatus();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ubuntu.exe",
                Arguments = "",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process = Process.Start(startInfo);

            _dataObject = LoadJsonFromDataFile();
            if (_isFreshStart || string.IsNullOrEmpty(_dataObject.Configs.ServerAdress) || string.IsNullOrEmpty(_dataObject.Configs.ClientAdress))
            {
                ErrorText.Visible = true;
                foreach (Control control in Controls)
                {
                    control.Enabled = false;
                }
                return;
            }

            ClientPathTextBox.Text = _dataObject.Configs.ClientTomlPath;
            ServerPathTextBox.Text = _dataObject.Configs.ServerTomlPath;

            for (int i = 0; i < _dataObject.Services.Count; i++)
            {
                var service = _dataObject.Services[i];
                var serviceControl = new ServiceControl();
                serviceControl.SetupControl(service);
                serviceControl.SetIndex(i);
                serviceControl.ExitClicked += ControlDeleted;
                serviceControl.Location = new Point(0, i * 130);
                panel1.Controls.Add(serviceControl);
            }

            _clientConnector = new SshConnector(_dataObject.Configs.ClientAdress.Split(':')[0],
                int.Parse(_dataObject.Configs.ClientAdress.Split(':')[1]), _dataObject.Configs.ClientUsername,
                _dataObject.Configs.ClientPassword);

            _serverConnector = new SshConnector(_dataObject.Configs.ServerAdress.Split(':')[0],
                int.Parse(_dataObject.Configs.ServerAdress.Split(':')[1]), _dataObject.Configs.ServerUsername,
                _dataObject.Configs.ServerPassword);

            StatusLabel.Text = "Status: checking rathole...";
            RunButton.Enabled = false;
            Task.Run(() =>
            {
                CheckRatholeBinaries();
                BeginInvoke(() =>
                {
                    if (_clientBinaryOk && _serverBinaryOk && !_clientSshFailed && !_serverSshFailed)
                        RefreshStatus();
                    else
                        ApplyBinaryStatus();
                    _statusTimer.Start();
                });
            });
        }

        private void CheckRatholeBinaries()
        {
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
                StatusLabel.Text = "Status: cannot check rathole (SSH error)";
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
                var tmpFile = File.Create(Program.DataFilePath);
                tmpFile.Close();
                _dataObject = new JsonDataClass();
                _dataObject.Configs = new ConfigStore();
                _dataObject.Services = new List<Service>();
                SaveData();
                _isFreshStart = true;
                return _dataObject;
            }

            try
            {
                string jsonFromFile = File.ReadAllText(Program.DataFilePath);
                var data = JsonSerializer.Deserialize<JsonDataClass>(jsonFromFile);

                if (data.Services == null)
                    data.Services = new List<Service>();
                if (data.Configs == null)
                    data.Configs = new ConfigStore();

                return data;
            }
            catch (Exception)
            {
                throw new FileLoadException("No file");
            }
        }

        public void ControlDeleted(int index, ServiceControl service)
        {
            panel1.SuspendLayout();
            _dataObject.Services.Remove(_dataObject.Services[index]);
            panel1.Controls.Remove(service);
            RearangeElements(index);
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
        }

        public void AddControl()
        {
            panel1.SuspendLayout();
            var service = new Service();
            var serviceControl = new ServiceControl();
            _dataObject.Services.Add(service);
            var index = _dataObject.Services.IndexOf(service);
            serviceControl.SetIndex(index);
            serviceControl.ExitClicked += ControlDeleted;
            serviceControl.Location = new Point(5, index * 130);
            panel1.Controls.Add(serviceControl);
            RearangeElements(index);
            panel1.ResumeLayout(true);
            panel1.PerformLayout();
        }

        private void RearangeElements(int index)
        {
            var controlsArray = panel1.Controls.OfType<ServiceControl>().ToList();
            for (int i = 0; i < controlsArray.Count; i++)
            {
                var serviceControl = controlsArray[i];
                serviceControl.Location = new Point(0, i * 130);
                serviceControl.Index = i;
            }
        }

        public void SaveData()
        {
            _dataObject.Configs.ClientTomlPath = ClientPathTextBox.Text;
            _dataObject.Configs.ServerTomlPath = ServerPathTextBox.Text;
            foreach (Control control in panel1.Controls)
            {
                if (control is not ServiceControl) continue;
                var castedControl = (ServiceControl)control;
                _dataObject.Services[castedControl.Index] = castedControl.GetServiceData();
            }

            string dataToWrite = JsonSerializer.Serialize(_dataObject, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Program.DataFilePath, dataToWrite);
        }

        private void AddRuleButton_Click(object sender, EventArgs e)
        {
            AddControl();
        }

        private void SaveRulesButton_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        public void WriteRulesToFile()
        {
            string clientFilePath = ClientPathTextBox.Text;
            string serverFilePath = ServerPathTextBox.Text;

            string serverfileContent = $@"[server]
bind_addr = ""0.0.0.0:2333""
heartbeat_interval = 20

[server.transport]
type = ""tcp""

[server.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

";

            string clientfileContent = $@"
[client]
remote_addr = ""{_dataObject.Configs.ServerAdress.Split(':')[0]}:2333""

[client.transport]
type = ""tcp""

[client.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

";

            File.WriteAllText(clientFilePath, clientfileContent, Encoding.UTF8);
            File.WriteAllText(serverFilePath, serverfileContent, Encoding.UTF8);
            var controlsArray = panel1.Controls.OfType<ServiceControl>().ToList();
            foreach (ServiceControl control in controlsArray)
            {
                var data = control.GetServiceData();
                string clienttext = $@"[client.services.{data.ServiceName}]
type = ""tcp""
token = ""{data.ServiceToken}""
local_addr = ""{data.ClientAdress}:{data.ClientPort}""
nodelay = {data.NoDelay.ToString().ToLower()}

";
                if (data.UdpEnabled)
                {
                    clienttext += $@"[client.services.{data.ServiceName}_udp]
type = ""udp""
token = ""{data.ServiceToken}_udp""
local_addr = ""{data.ClientAdress}:{data.ClientPort}""

";
                }

                string servertext = $@"[server.services.{data.ServiceName}]
type = ""tcp""
token = ""{data.ServiceToken}""
bind_addr = ""{data.ServerAdress}:{data.ServerPort}""
nodelay = {data.NoDelay.ToString().ToLower()}

";
                if (data.UdpEnabled)
                {
                    servertext += $@"[server.services.{data.ServiceName}_udp]
type = ""udp""
token = ""{data.ServiceToken}_udp""
bind_addr = ""{data.ServerAdress}:{data.ServerPort}""

";
                }

                File.AppendAllText(clientFilePath, clienttext, Encoding.UTF8);
                File.AppendAllText(serverFilePath, servertext, Encoding.UTF8);
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            CheckRatholeBinaries();
            if (!_clientBinaryOk || !_serverBinaryOk || _clientSshFailed || _serverSshFailed)
            {
                ApplyBinaryStatus();
                return;
            }

            WriteRulesToFile();

            _serverConnector.SendFile(_dataObject.Configs.ServerTomlPath,
                _dataObject.Configs.ServerRatholePath + _dataObject.Configs.ServerTomlPath.Split('/')[^1]);
            _serverConnector.BeginRatholeConnection(_dataObject.Configs.ServerRatholePath + _dataObject.Configs.ServerTomlPath.Split('/')[^1],
                _dataObject.Configs.ServerRatholePath);
            _clientConnector.SendFile(_dataObject.Configs.ClientTomlPath,
                _dataObject.Configs.ClientRatholePath + _dataObject.Configs.ClientTomlPath.Split('/')[^1]);
            _clientConnector.BeginRatholeConnection(_dataObject.Configs.ClientRatholePath + _dataObject.Configs.ClientTomlPath.Split('/')[^1],
                _dataObject.Configs.ClientRatholePath);

            RefreshStatus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _statusTimer.Stop();
            _clientConnector?.EndRatholeConnection();
            _serverConnector?.EndRatholeConnection();
            process?.Kill(true);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _clientConnector.EndRatholeConnection();
            _serverConnector.EndRatholeConnection();
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (!_clientBinaryOk || !_serverBinaryOk || _clientSshFailed || _serverSshFailed)
            {
                ApplyBinaryStatus();
                return;
            }

            bool clientUp = _clientConnector != null && _clientConnector.IsRatholeRunning();
            bool serverUp = _serverConnector != null && _serverConnector.IsRatholeRunning();
            bool running = clientUp && serverUp;

            StopButton.Enabled = running;
            RunButton.Enabled = !running;
            pictureBox1.Image = running ? SuccessImage : ErrorImage;
            StatusLabel.Text = running ? "Status: Running" : "Status: Stopped";
        }
    }

    public class JsonDataClass
    {
        public ConfigStore Configs { get; set; }
        public List<Service> Services { get; set; }
    }
}
