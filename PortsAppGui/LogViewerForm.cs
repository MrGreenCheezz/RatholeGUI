using PortsAppGui.UI;
using Timer = System.Windows.Forms.Timer;

namespace PortsAppGui
{
    public class LogViewerForm : Form
    {
        private readonly Func<string> _loadServerLog;
        private readonly Func<string> _loadClientLog;
        private readonly ModernButton _serverTab;
        private readonly ModernButton _clientTab;
        private readonly ModernButton _refreshButton;
        private readonly ModernCheckBox _autoRefresh;
        private readonly Label _statusLabel;
        private readonly TextBox _viewer;
        private readonly Timer _timer;

        private bool _showingServer = true;
        private bool _isLoading;

        public LogViewerForm(Func<string> loadServerLog, Func<string> loadClientLog)
        {
            _loadServerLog = loadServerLog;
            _loadClientLog = loadClientLog;
            _timer = new Timer { Interval = 5000 };

            Text = "Rathole logs";
            ClientSize = new Size(1000, 700);
            MinimumSize = new Size(680, 440);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Icon = Theme.LoadAppIcon();
            Theme.ApplyTo(this);

            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Theme.Surface };

            _serverTab = new ModernButton
            {
                Text = "Server log",
                Variant = ButtonVariant.Accent,
                Size = new Size(130, 34),
                Location = new Point(20, 15)
            };
            _clientTab = new ModernButton
            {
                Text = "Client log",
                Variant = ButtonVariant.Ghost,
                Size = new Size(130, 34),
                Location = new Point(158, 15)
            };
            _serverTab.Click += async (_, _) => await SwitchTo(server: true);
            _clientTab.Click += async (_, _) => await SwitchTo(server: false);

            _autoRefresh = new ModernCheckBox
            {
                Text = "Auto-refresh",
                Location = new Point(310, 20)
            };
            _autoRefresh.CheckedChanged += (_, _) =>
            {
                if (_autoRefresh.Checked)
                    _timer.Start();
                else
                    _timer.Stop();
            };

            _refreshButton = new ModernButton
            {
                Text = "Refresh",
                Glyph = Glyphs.Refresh,
                Variant = ButtonVariant.Standard,
                Size = new Size(120, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Top = 15
            };
            _refreshButton.Click += async (_, _) => await RefreshLogs();
            header.Resize += (_, _) => _refreshButton.Left = header.ClientSize.Width - 20 - _refreshButton.Width;

            var separator = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.Border };

            header.Controls.Add(_serverTab);
            header.Controls.Add(_clientTab);
            header.Controls.Add(_autoRefresh);
            header.Controls.Add(_refreshButton);
            header.Controls.Add(separator);

            _viewer = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = new Font(Theme.PickMonoFontFamily(), 10F)
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 0), BackColor = Theme.Window };
            body.Controls.Add(_viewer);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Theme.Window, Padding = new Padding(20, 10, 20, 12) };
            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = Theme.Caption,
                ForeColor = Theme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };
            footer.Controls.Add(_statusLabel);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);

            _timer.Tick += async (_, _) => await RefreshLogs();

            Shown += async (_, _) =>
            {
                Theme.ApplyDarkScrollBars(_viewer);
                await RefreshLogs();
            };
            FormClosed += (_, _) => _timer.Dispose();
        }

        private async Task SwitchTo(bool server)
        {
            _showingServer = server;
            _serverTab.Variant = server ? ButtonVariant.Accent : ButtonVariant.Ghost;
            _clientTab.Variant = server ? ButtonVariant.Ghost : ButtonVariant.Accent;
            _serverTab.Invalidate();
            _clientTab.Invalidate();
            await RefreshLogs();
        }

        private async Task RefreshLogs()
        {
            if (_isLoading)
                return;

            _isLoading = true;
            _refreshButton.Enabled = false;
            _statusLabel.Text = "Loading…";

            var loader = _showingServer ? _loadServerLog : _loadClientLog;

            try
            {
                // The loaders open an SSH session; keep that off the UI thread.
                var text = await Task.Run(() =>
                {
                    try
                    {
                        return loader();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                });

                if (IsDisposed)
                    return;

                _viewer.Text = text;
                ScrollToBottom();

                var lines = string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
                _statusLabel.Text = $"{(_showingServer ? "Server" : "Client")} log — {lines} line(s)";
            }
            finally
            {
                _isLoading = false;
                if (!IsDisposed)
                    _refreshButton.Enabled = true;
            }
        }

        private void ScrollToBottom()
        {
            _viewer.SelectionStart = _viewer.TextLength;
            _viewer.ScrollToCaret();
        }
    }
}
