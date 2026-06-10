namespace PortsAppGui
{
    internal static class Program
    {
        internal static readonly string DataFilePath = "./data.json";
        internal static readonly string ConnectionStateFilePath = "./connection-state.json";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
