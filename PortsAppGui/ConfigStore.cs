using System.Text.Json.Serialization;

namespace PortsAppGui
{
    public class ConfigStore
    {
        public string ServerTomlPath { get; set; } = "";
        public string ClientTomlPath { get; set; } = "";

        [JsonPropertyName("ServerAdress")]
        public string ServerAddress { get; set; } = "";

        [JsonPropertyName("ClientAdress")]
        public string ClientAddress { get; set; } = "";

        public string ServerRatholePath { get; set; } = "";
        public string ClientRatholePath { get; set; } = "";
        public string ServerUsername { get; set; } = "";
        public string ClientUsername { get; set; } = "";
        public string ServerPassword { get; set; } = "";
        public string ClientPassword { get; set; } = "";

        /// <summary>Port the rathole server listens on for its clients (server bind_addr / client remote_addr).</summary>
        public string ControlPort { get; set; } = TomlGenerator.DefaultControlPort;

        /// <summary>Leave rathole running on both machines when the app exits.</summary>
        public bool KeepTunnelOnExit { get; set; }
    }
}
