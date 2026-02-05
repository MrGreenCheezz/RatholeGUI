
using System;
using System.Linq;
using Renci.SshNet;

namespace PortsAppGui
{
 
    public class SshConnector
    {
        string _host = "your.server.com";
        int _port = 22;
        string _username = "user";
        string _password = "password";

        public SshClient Client;
        public string ProccessPID;

        public SshConnector(string host, int port, string username, string password)
        { 
           _host = host;
           _port = port;
           _username = username;
           _password = password;
        }

        public void SendFile(string localFilePath, string remoteFilePath)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                client.Connect();
                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    client.UploadFile(fileStream, remoteFilePath);
                }
                client.Disconnect();
            }
        }

        public void BeginRatholeConnection(string remoteFilePath, string ratholeFilePath)
        {
            var ssh = new SshClient(_host, _port, _username, _password);
            ssh.KeepAliveInterval = TimeSpan.FromSeconds(60);
            ssh.Connect(); 
            var command = "nohup " + ratholeFilePath + "./rathole " + remoteFilePath + " >rathole.log 2>&1 & echo $!";
            var cmdResult = ssh.RunCommand(command);
            ProccessPID = cmdResult.Result.Trim();
            Client = ssh;
        }

        public void EndRatholeConnection()
        {
            if (Client == null) return;
            if (!Client.IsConnected)
            {
                Client.Connect();
            }

            if (!string.IsNullOrEmpty(ProccessPID))
            {
                Client.RunCommand($"kill -9 {ProccessPID}");
                ProccessPID = "";
            }
            
            Client?.Disconnect();
        }

        public bool IsRatholeRunning(IEnumerable<int> ports)
        {
            if (Client == null)
            {
                return false;
            }

            if (!Client.IsConnected)
            {
                Client.Connect();
            }

            var processRunning = false;
            if (!string.IsNullOrWhiteSpace(ProccessPID))
            {
                var pidCheck = Client.RunCommand($"ps -p {ProccessPID} -o comm=");
                processRunning = pidCheck.ExitStatus == 0 && pidCheck.Result.Trim().Contains("rathole", StringComparison.OrdinalIgnoreCase);
            }

            if (!processRunning)
            {
                var pgrepCheck = Client.RunCommand("pgrep -x rathole");
                processRunning = !string.IsNullOrWhiteSpace(pgrepCheck.Result);
            }

            var portList = ports?
                .Where(port => port > 0 && port != 22)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (portList.Length == 0)
            {
                return processRunning;
            }

            var portRegex = string.Join("|", portList);
            var portCheck = Client.RunCommand($"ss -ltnup 2>/dev/null | grep -E \"rathole\" | grep -E \":({portRegex})\\\\b\"");
            if (!string.IsNullOrWhiteSpace(portCheck.Result))
            {
                return true;
            }

            return processRunning;
        }
    }

}
