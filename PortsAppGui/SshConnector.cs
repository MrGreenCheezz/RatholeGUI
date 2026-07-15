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

        private static string ShellQuote(string path) => "'" + path.Replace("'", "'\\''") + "'";

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public SshClient? Client;
        public string ProcessPid { get; private set; } = "";

        public SshConnector(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        public void LoadProcessPid(string processPid)
        {
            ProcessPid = processPid.All(char.IsDigit) ? processPid : "";
        }

        public void SendFile(string localFilePath, string remoteFilePath)
        {
            using var client = new SftpClient(_host, _port, _username, _password);
            client.Connect();

            using var fileStream = new FileStream(localFilePath, FileMode.Open);
            client.UploadFile(fileStream, remoteFilePath);

            client.Disconnect();
        }

        public void BeginRatholeConnection(string remoteFilePath, string ratholeDir)
        {
            var ssh = new SshClient(_host, _port, _username, _password)
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60)
            };
            ssh.Connect();

            var ratholeBinary = GetRatholeBinaryPath(ratholeDir);
            var command = $"nohup {ShellQuote(ratholeBinary)} {ShellQuote(remoteFilePath)} > rathole.log 2>&1 & echo $!";
            var cmdResult = ssh.RunCommand(command);
            ProcessPid = cmdResult.Result.Trim();
            Client = ssh;
        }

        public bool TestConnection(out string error)
        {
            error = "";
            try
            {
                using var ssh = new SshClient(_host, _port, _username, _password);
                ssh.Connect();
                var result = ssh.RunCommand("echo ok");
                if (result.ExitStatus != 0)
                {
                    error = result.Error.Trim();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public string ReadRatholeLog(int lines = 100)
        {
            using var ssh = new SshClient(_host, _port, _username, _password);
            ssh.Connect();
            var command = $"tail -n {Math.Clamp(lines, 1, 1000)} rathole.log 2>&1";
            var result = ssh.RunCommand(command);
            return string.IsNullOrWhiteSpace(result.Result) ? result.Error : result.Result;
        }

        public void StopAllRatholeProcesses()
        {
            var ownsClient = Client == null;
            var ssh = Client ?? new SshClient(_host, _port, _username, _password);

            try
            {
                if (!ssh.IsConnected)
                    ssh.Connect();

                var result = ssh.RunCommand("""
                    command -v pkill >/dev/null 2>&1 && command -v pgrep >/dev/null 2>&1 || { echo 'pkill/pgrep not found' >&2; exit 127; }
                    if pgrep -x rathole >/dev/null 2>&1; then
                        pkill -TERM -x rathole 2>/dev/null || true
                        sleep 1
                        pkill -KILL -x rathole 2>/dev/null || true
                    fi
                    if pgrep -x rathole >/dev/null 2>&1; then
                        echo 'rathole processes remain (check permissions)' >&2
                        exit 1
                    fi
                    """);
                if (result.ExitStatus != 0)
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error)
                        ? "Failed to stop all rathole processes."
                        : result.Error.Trim());

                ProcessPid = "";
            }
            finally
            {
                if (ownsClient)
                    ssh.Dispose();
                else
                    ssh.Disconnect();
            }
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
            try
            {
                if (Client != null)
                {
                    if (!Client.IsConnected)
                        Client.Connect();

                    return IsRatholeRunning(Client);
                }

                using var ssh = new SshClient(_host, _port, _username, _password);
                ssh.Connect();
                return IsRatholeRunning(ssh);
            }
            catch
            {
                return false;
            }
        }

        private bool IsRatholeRunning(SshClient ssh)
        {
            if (!string.IsNullOrEmpty(ProcessPid) && ProcessPid.All(char.IsDigit))
            {
                var pidCheck = ssh.RunCommand($"ps -p {ProcessPid} -o comm=");
                if (pidCheck.ExitStatus == 0 && pidCheck.Result.Contains("rathole"))
                    return true;
            }

            var pgrep = ssh.RunCommand("pgrep -x rathole");
            return !string.IsNullOrWhiteSpace(pgrep.Result);
        }
    }
}
