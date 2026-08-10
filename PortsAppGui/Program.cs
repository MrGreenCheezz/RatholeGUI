using PortsAppGui.UI;

namespace PortsAppGui
{
    internal static class Program
    {
        internal static readonly string DataFilePath = "./data.json";
        internal static readonly string ConnectionStateFilePath = "./connection-state.json";
        internal static readonly string CrashLogPath = "./ratholegui-error.log";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Theme.EnableProcessDarkMode();

            Application.ThreadException += (_, e) => ReportCrash(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportCrash(e.ExceptionObject as Exception);

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                ReportCrash(ex);
            }
        }

        /// <summary>Writes the failure next to the executable and tells the user, instead of vanishing silently.</summary>
        private static void ReportCrash(Exception? exception)
        {
            if (exception == null)
                return;

            try
            {
                File.AppendAllText(CrashLogPath, $"[{DateTime.Now:u}] {exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // Nothing else we can do if even logging fails.
            }

            try
            {
                Dialogs.Error(null, "RatholeGUI crashed",
                    $"{exception.Message}{Environment.NewLine}{Environment.NewLine}Details were written to {CrashLogPath}.");
            }
            catch
            {
                // The handler can fire on a background thread where no dialog can be shown.
            }
        }
    }
}
