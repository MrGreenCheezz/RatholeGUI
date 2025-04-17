using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using Renci.SshNet;
using Renci.SshNet.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
            if (!string.IsNullOrEmpty(ProccessPID))
            {
                Client.RunCommand($"kill -9 {ProccessPID}");
                ProccessPID = "";
            }
            
            Client?.Disconnect();
        }
    }

}
