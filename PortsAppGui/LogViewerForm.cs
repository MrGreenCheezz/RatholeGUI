namespace PortsAppGui
{
    public class LogViewerForm : Form
    {
        private readonly Func<string> _loadServerLog;
        private readonly Func<string> _loadClientLog;
        private readonly TextBox _serverLogTextBox = CreateLogBox();
        private readonly TextBox _clientLogTextBox = CreateLogBox();

        public LogViewerForm(Func<string> loadServerLog, Func<string> loadClientLog)
        {
            _loadServerLog = loadServerLog;
            _loadClientLog = loadClientLog;

            Text = "Rathole logs";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;

            var refreshButton = new Button
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "Refresh logs"
            };
            refreshButton.Click += (_, _) => RefreshLogs();

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var serverPage = new TabPage("Server log");
            serverPage.Controls.Add(_serverLogTextBox);
            var clientPage = new TabPage("Client log");
            clientPage.Controls.Add(_clientLogTextBox);
            tabs.TabPages.Add(serverPage);
            tabs.TabPages.Add(clientPage);

            Controls.Add(tabs);
            Controls.Add(refreshButton);
            Shown += (_, _) => RefreshLogs();
        }

        private void RefreshLogs()
        {
            _serverLogTextBox.Text = SafeLoad(_loadServerLog);
            _clientLogTextBox.Text = SafeLoad(_loadClientLog);
        }

        private static string SafeLoad(Func<string> loader)
        {
            try
            {
                return loader();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static TextBox CreateLogBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 10)
            };
        }
    }
}
