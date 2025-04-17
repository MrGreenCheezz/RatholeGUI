using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace PortsAppGui
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        internal static readonly string DataFilePath = "./data.json";
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,

            // see https://aka.ms/applicationconfiguration.
            
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

           
        }
    }
}