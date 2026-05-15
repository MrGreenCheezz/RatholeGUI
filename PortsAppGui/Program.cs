namespace PortsAppGui
{
    internal static class Program
    {
        internal static readonly string DataFilePath = "./data.json";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
