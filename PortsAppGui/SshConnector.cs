using System;
using System.Linq;
using Renci.SshNet;

namespace PortsAppGui
{
    public class SshConnector
    {
        private readonly string _host = "your.server.com";
        private readonly int _port = 22;
        private readonly string _username = "user";
        private readonly string _password = "password";

        public SshClient Client;
        public string ProccessPID;
        private readonly object _lockObj = new object();

        public SshConnector(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        // Safely connect or reconnect SSH client
        private void EnsureConnected()
        {
            lock (_lockObj)
            {
                try
                {
                    if (Client == null || !Client.IsConnected)
                    {
                        // Dispose old client if exists
                        Client?.Disconnect();
                        Client?.Dispose();

                        // Create new connection
                        Client = new SshClient(_host, _port, _username, _password);
                        Client.KeepAliveInterval = TimeSpan.FromSeconds(60);
                        Client.Connect();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SSH] Connection failed: {ex.Message}");
                    Client = null;
                    throw;
                }
            }
        }

        // Execute command with automatic reconnection and error handling
        private SshCommand ExecuteCommandSafe(string command, int maxRetries = 2)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    lock (_lockObj)
                    {
                        EnsureConnected();
                        var cmd = Client.RunCommand(command);
                        return cmd;
                    }
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"[SSH] Command execution failed (attempt {attempt}/{maxRetries}): {ex.Message}");
                    Client = null; // Force reconnection on next attempt

                    if (attempt >= maxRetries)
                    {
                        throw new Exception($"SSH command failed after {maxRetries} attempts: {ex.Message}", ex);
                    }

                    // Small delay before retry
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new Exception($"SSH command failed after {maxRetries} attempts");
        }

        public void SendFile(string localFilePath, string remoteFilePath)
        {
            try
            {
                using (var client = new SftpClient(_host, _port, _username, _password))
                {
                    client.Connect();
                    using (var fileStream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
                    {
                        client.UploadFile(fileStream, remoteFilePath);
                    }
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SFTP] File upload failed: {ex.Message}");
                throw;
            }
        }

        public void BeginRatholeConnection(string remoteFilePath, string ratholeFilePath)
        {
            try
            {
                EnsureConnected();
                var command = "nohup " + ratholeFilePath + "./rathole " + remoteFilePath + " >rathole.log 2>&1 & echo $!";
                var cmdResult = ExecuteCommandSafe(command);
                ProccessPID = cmdResult.Result.Trim();
                Console.WriteLine($"[SSH] Rathole process started with PID: {ProccessPID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SSH] Failed to start Rathole: {ex.Message}");
                throw;
            }
        }

        public void EndRatholeConnection()
        {
            try
            {
                lock (_lockObj)
                {
                    if (Client == null)
                        return;

                    if (!Client.IsConnected)
                    {
                        EnsureConnected();
                    }

                    if (!string.IsNullOrEmpty(ProccessPID))
                    {
                        try
                        {
                            var killCmd = Client.RunCommand($"kill -9 {ProccessPID}");
                            Console.WriteLine($"[SSH] Process killed: {ProccessPID}");
                            ProccessPID = "";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SSH] Failed to kill process: {ex.Message}");
                        }
                    }

                    Client?.Disconnect();
                    Client?.Dispose();
                    Client = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SSH] Error ending connection: {ex.Message}");
            }
        }

        public bool IsRatholeRunning(IEnumerable<int> ports)
        {
            try
            {
                // Ensure we have a valid connection
                EnsureConnected();

                var processRunning = false;

                // Check by PID first if we have one
                if (!string.IsNullOrWhiteSpace(ProccessPID))
                {
                    try
                    {
                        var pidCheck = ExecuteCommandSafe($"ps -p {ProccessPID} -o comm=");
                        processRunning = pidCheck.ExitStatus == 0 && pidCheck.Result.Trim().Contains("rathole", StringComparison.OrdinalIgnoreCase);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SSH] PID check failed: {ex.Message}");
                    }
                }

                // If not running by PID, check by process name
                if (!processRunning)
                {
                    try
                    {
                        var pgrepCheck = ExecuteCommandSafe("pgrep -x rathole");
                        processRunning = !string.IsNullOrWhiteSpace(pgrepCheck.Result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SSH] Pgrep check failed: {ex.Message}");
                    }
                }

                // Check ports if we have them
                var portList = ports?
                    .Where(port => port > 0 && port != 22)
                    .Distinct()
                    .ToArray() ?? Array.Empty<int>();

                if (portList.Length > 0 && !processRunning)
                {
                    try
                    {
                        var portRegex = string.Join("|", portList);
                        var portCheck = ExecuteCommandSafe($"ss -ltnup 2>/dev/null | grep -E \"rathole\" | grep -E \":({portRegex})\\\\b\"");
                        if (!string.IsNullOrWhiteSpace(portCheck.Result))
                        {
                            processRunning = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SSH] Port check failed: {ex.Message}");
                    }
                }

                return processRunning;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SSH] IsRatholeRunning failed: {ex.Message}");
                return false; // Safely return false instead of crashing
            }
        }
    }
}