namespace PortsAppGui
{
    public class Service
    {
        public string ServiceName { get; set; }
        public string ServiceToken { get; set; }
        public string ClientAdress { get; set; }
        public string ClientPort { get; set; }
        public string ServerAdress { get; set; }
        public string ServerPort { get; set; }
        public bool NoDelay { get; set; } = true;
        public bool UdpEnabled { get; set; } = true;
    }
}
