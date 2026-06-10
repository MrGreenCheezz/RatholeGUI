using System.Text.Json;

namespace PortsAppGui
{
    public class ConnectionState
    {
        public string ServerAddress { get; set; } = "";
        public string ClientAddress { get; set; } = "";
        public string ServerPid { get; set; } = "";
        public string ClientPid { get; set; } = "";

        public bool Matches(ConfigStore config)
        {
            return string.Equals(ServerAddress, config.ServerAddress, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ClientAddress, config.ClientAddress, StringComparison.OrdinalIgnoreCase);
        }

        public static ConnectionState? Load()
        {
            try
            {
                if (!File.Exists(Program.ConnectionStateFilePath))
                    return null;

                var json = File.ReadAllText(Program.ConnectionStateFilePath);
                return JsonSerializer.Deserialize<ConnectionState>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void Save(ConnectionState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Program.ConnectionStateFilePath, json);
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(Program.ConnectionStateFilePath))
                    File.Delete(Program.ConnectionStateFilePath);
            }
            catch
            {
            }
        }
    }
}
