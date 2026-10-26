using System.Text;

namespace PortsAppGui
{
    public static class TomlGenerator
    {
        public const string DefaultControlPort = "2333";

        public static string GenerateServerConfig(ConfigStore config, IEnumerable<Service> services)
        {
            var builder = new StringBuilder($@"[server]
bind_addr = {Address("0.0.0.0", config.ControlPort)}
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
                builder.Append($@"[server.services.{Key(data.ServiceName)}]
type = ""tcp""
token = {Quote(data.ServiceToken)}
bind_addr = {Address(data.ServerAddress, data.ServerPort)}
nodelay = {data.NoDelay.ToString().ToLower()}

");
                if (data.UdpEnabled)
                {
                    builder.Append($@"[server.services.{Key(data.ServiceName + "_udp")}]
type = ""udp""
token = {Quote(data.ServiceToken + "_udp")}
bind_addr = {Address(data.ServerAddress, data.ServerPort)}

");
                }
            }

            return builder.ToString();
        }

        public static string GenerateClientConfig(ConfigStore config, IEnumerable<Service> services)
        {
            var serverHost = config.ServerAddress.Split(':')[0];
            var builder = new StringBuilder($@"[client]
remote_addr = {Address(serverHost, config.ControlPort)}

[client.transport]
type = ""tcp""

[client.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

");

            foreach (var data in services.Where(service => service.Enabled))
            {
                builder.Append($@"[client.services.{Key(data.ServiceName)}]
type = ""tcp""
token = {Quote(data.ServiceToken)}
local_addr = {Address(data.ClientAddress, data.ClientPort)}
nodelay = {data.NoDelay.ToString().ToLower()}

");
                if (data.UdpEnabled)
                {
                    builder.Append($@"[client.services.{Key(data.ServiceName + "_udp")}]
type = ""udp""
token = {Quote(data.ServiceToken + "_udp")}
local_addr = {Address(data.ClientAddress, data.ClientPort)}

");
                }
            }

            return builder.ToString();
        }

        /// <summary>A TOML basic string: quotes, backslashes and control characters are escaped.</summary>
        public static string Quote(string value)
        {
            var builder = new StringBuilder(value.Length + 2).Append('"');
            foreach (var character in value)
            {
                switch (character)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\t': builder.Append("\\t"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\r': builder.Append("\\r"); break;
                    default:
                        if (char.IsControl(character))
                            builder.Append($"\\u{(int)character:X4}");
                        else
                            builder.Append(character);
                        break;
                }
            }

            return builder.Append('"').ToString();
        }

        /// <summary>Validated names stay bare keys; anything else is quoted so it can't break the table header.</summary>
        private static string Key(string name)
        {
            return ConfigValidator.IsValidServiceName(name) ? name : Quote(name);
        }

        private static string Address(string host, string port)
        {
            return Quote(host.Trim() + ":" + port.Trim());
        }
    }
}
