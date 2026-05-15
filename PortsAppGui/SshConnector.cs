using Renci.SshNet;

namespace PortsAppGui
{
    public class SshConnector
    {
        public static string GetRatholeBinaryPath(string ratholeDir)
        {
            if (string.IsNullOrWhiteSpace(ratholeDir))
                return "";
            return ratholeDir.TrimEnd('/') + "/rathole";
        }

        static string ShellQuote(string path) => "'" + path.Replace("'", "'\\''") + "'";
        string _host;
        int _port;
        string _username;
        string _password;

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

        // null = SSH error, true = exists, false = missing
        public bool? RatholeBinaryExists(string ratholeDir)
        {
            var path = GetRatholeBinaryPath(ratholeDir);
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                using var ssh = new SshClient(_host, _port, _username, _password);
                ssh.Connect();
                var result = ssh.RunCommand($"test -x {ShellQuote(path)}");
                return result.ExitStatus == 0;
            }
            catch
            {
                return null;
            }
        }

        public bool IsRatholeRunning()
        {
            if (Client == null)
                return false;

            if (!Client.IsConnected)
                Client.Connect();

            if (!string.IsNullOrEmpty(ProccessPID))
            {
                var pidCheck = Client.RunCommand($"ps -p {ProccessPID} -o comm=");
                if (pidCheck.ExitStatus == 0 && pidCheck.Result.Contains("rathole"))
                    return true;
            }

            var pgrep = Client.RunCommand("pgrep -x rathole");
            return !string.IsNullOrWhiteSpace(pgrep.Result);
        }
    }
}
