using System.Text.Json.Serialization;

namespace PortsAppGui
{
    public class Service
    {
        public string ServiceName { get; set; } = "";
        public string ServiceToken { get; set; } = "";

        [JsonPropertyName("ClientAdress")]
        public string ClientAddress { get; set; } = "";

        public string ClientPort { get; set; } = "";

        [JsonPropertyName("ServerAdress")]
        public string ServerAddress { get; set; } = "";

        public string ServerPort { get; set; } = "";
        public bool NoDelay { get; set; } = true;
        public bool UdpEnabled { get; set; } = true;
        public bool Enabled { get; set; } = true;
    }
}
