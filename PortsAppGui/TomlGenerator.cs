using System.Text;

namespace PortsAppGui
{
    public static class TomlGenerator
    {
        public static string GenerateServerConfig(IEnumerable<Service> services)
        {
            var builder = new StringBuilder(@"[server]
bind_addr = ""0.0.0.0:2333""
heartbeat_interval = 20

[server.transport]
type = ""tcp""

[server.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

");

            foreach (var data in services.Where(service => service.Enabled))
            {
                builder.Append($@"[server.services.{data.ServiceName}]
type = ""tcp""
token = ""{data.ServiceToken}""
bind_addr = ""{data.ServerAddress}:{data.ServerPort}""
nodelay = {data.NoDelay.ToString().ToLower()}

");
                if (data.UdpEnabled)
                {
                    builder.Append($@"[server.services.{data.ServiceName}_udp]
type = ""udp""
token = ""{data.ServiceToken}_udp""
bind_addr = ""{data.ServerAddress}:{data.ServerPort}""

");
                }
            }

            return builder.ToString();
        }

        public static string GenerateClientConfig(ConfigStore config, IEnumerable<Service> services)
        {
            var serverHost = config.ServerAddress.Split(':')[0];
            var builder = new StringBuilder($@"[client]
remote_addr = ""{serverHost}:2333""

[client.transport]
type = ""tcp""

[client.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

");

            foreach (var data in services.Where(service => service.Enabled))
            {
                builder.Append($@"[client.services.{data.ServiceName}]
type = ""tcp""
token = ""{data.ServiceToken}""
local_addr = ""{data.ClientAddress}:{data.ClientPort}""
nodelay = {data.NoDelay.ToString().ToLower()}

");
                if (data.UdpEnabled)
                {
                    builder.Append($@"[client.services.{data.ServiceName}_udp]
type = ""udp""
token = ""{data.ServiceToken}_udp""
local_addr = ""{data.ClientAddress}:{data.ClientPort}""

");
                }
            }

            return builder.ToString();
        }
    }
}
