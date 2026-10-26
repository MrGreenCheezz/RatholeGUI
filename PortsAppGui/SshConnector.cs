using Renci.SshNet;

namespace PortsAppGui
{
    /// <summary>
    /// One machine reached over SSH. A single session is kept open and reused by every call, so
    /// the status poll does not log in again every few seconds. All methods block: call them off
    /// the UI thread.
    /// </summary>
    public sealed class SshConnector : IDisposable
    {
        private const string LogFile = "rathole.log";
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

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
        private readonly object _sync = new();
        private SshClient? _ssh;
        private volatile bool _disposed;

        public string ProcessPid { get; private set; } = "";

        public SshConnector(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        public bool HasSameTarget(string host, int port, string username, string password)
        {
            return string.Equals(_host, host, StringComparison.OrdinalIgnoreCase) && _port == port &&
                   _username == username && _password == password;
        }

        public void LoadProcessPid(string processPid)
        {
            ProcessPid = processPid.All(char.IsDigit) ? processPid : "";
        }

        public void SendFile(string localFilePath, string remoteFilePath)
        {
            using var client = new SftpClient(_host, _port, _username, _password);
            client.ConnectionInfo.Timeout = ConnectTimeout;
            client.Connect();

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            client.UploadFile(fileStream, remoteFilePath);

            client.Disconnect();
        }

        public void StartRathole(string remoteFilePath, string ratholeDir)
        {
            var ratholeBinary = GetRatholeBinaryPath(ratholeDir);
            var command = $"nohup {ShellQuote(ratholeBinary)} {ShellQuote(remoteFilePath)} > {LogFile} 2>&1 & echo $!";
            var result = Run(ssh => ssh.RunCommand(command));
            var pid = result.Result.Trim();
            if (pid.Length == 0 || !pid.All(char.IsDigit))
                throw new InvalidOperationException($"Could not start rathole on {_host}: {result.Error.Trim()}");

            ProcessPid = pid;
        }

        /// <summary>
        /// Throws with the last lines of rathole.log when the process started by
        /// <see cref="StartRathole"/> is already gone (bad config, port in use, …).
        /// </summary>
        public void EnsureStarted()
        {
            if (Run(IsOwnProcessAlive))
                return;

            ProcessPid = "";
            var log = ReadRatholeLog(20).Trim();
            throw new InvalidOperationException(string.IsNullOrEmpty(log)
                ? $"rathole on {_host} exited right after start."
                : $"rathole on {_host} exited right after start. Last lines of {LogFile}:{Environment.NewLine}{log}");
        }

        public bool TestConnection(out string error)
        {
            error = "";
            try
            {
                using var ssh = CreateClient();
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
            var command = $"tail -n {Math.Clamp(lines, 1, 1000)} {LogFile} 2>&1";
            var result = Run(ssh => ssh.RunCommand(command));
            return string.IsNullOrWhiteSpace(result.Result) ? result.Error : result.Result;
        }

        public void StopAllRatholeProcesses()
        {
            var result = Run(ssh => ssh.RunCommand("""
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
                """));
            if (result.ExitStatus != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error)
                    ? "Failed to stop all rathole processes."
                    : result.Error.Trim());

            ProcessPid = "";
        }

        // null = SSH error, true = exists, false = missing
        public bool? RatholeBinaryExists(string ratholeDir)
        {
            var path = GetRatholeBinaryPath(ratholeDir);
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                return Run(ssh => ssh.RunCommand($"test -x {ShellQuote(path)}").ExitStatus == 0);
            }
            catch
            {
                return null;
            }
        }

        // null = SSH error
        public bool? IsRatholeRunning()
        {
            try
            {
                return Run(ssh => IsOwnProcessAlive(ssh) || !string.IsNullOrWhiteSpace(ssh.RunCommand("pgrep -x rathole").Result));
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            DropClient();
        }

        private bool IsOwnProcessAlive(SshClient ssh)
        {
            var pid = ProcessPid;
            if (string.IsNullOrEmpty(pid) || !pid.All(char.IsDigit))
                return false;

            var pidCheck = ssh.RunCommand($"ps -p {pid} -o comm=");
            return pidCheck.ExitStatus == 0 && pidCheck.Result.Contains("rathole");
        }

        /// <summary>Runs <paramref name="action"/> on the shared session, reconnecting if it dropped.</summary>
        private T Run<T>(Func<SshClient, T> action)
        {
            lock (_sync)
            {
                var ssh = GetConnectedClient();
                try
                {
                    return action(ssh);
                }
                catch
                {
                    // The session may be half-dead; the next call starts a fresh one.
                    DropClient();
                    throw;
                }
            }
        }

        private SshClient GetConnectedClient()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_ssh is { IsConnected: true } current)
                return current;

            DropClient();
            var ssh = CreateClient();
            ssh.KeepAliveInterval = TimeSpan.FromSeconds(30);
            try
            {
                ssh.Connect();
            }
            catch
            {
                ssh.Dispose();
                throw;
            }

            _ssh = ssh;
            if (_disposed)
            {
                // Dispose() ran on another thread while we were connecting.
                DropClient();
                throw new ObjectDisposedException(nameof(SshConnector));
            }

            return ssh;
        }

        private SshClient CreateClient()
        {
            var ssh = new SshClient(_host, _port, _username, _password);
            ssh.ConnectionInfo.Timeout = ConnectTimeout;
            return ssh;
        }

        private void DropClient()
        {
            Interlocked.Exchange(ref _ssh, null)?.Dispose();
        }
    }
}
