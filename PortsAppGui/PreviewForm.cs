using PortsAppGui.UI;

namespace PortsAppGui
{
    public class PreviewForm : Form
    {
        private readonly string _clientConfig;
        private readonly string _serverConfig;
        private readonly ModernButton _clientTab;
        private readonly ModernButton _serverTab;
        private readonly TextBox _viewer;
        private bool _showingClient = true;

        public PreviewForm(string clientConfig, string serverConfig)
        {
            _clientConfig = clientConfig;
            _serverConfig = serverConfig;

            Text = "Generated TOML preview";
            ClientSize = new Size(900, 660);
            MinimumSize = new Size(640, 420);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Icon = Theme.LoadAppIcon();
            Theme.ApplyTo(this);

            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Theme.Surface };

            _clientTab = new ModernButton
            {
                Text = "Client config",
                Variant = ButtonVariant.Accent,
                Size = new Size(140, 34),
                Location = new Point(20, 15)
            };
            _serverTab = new ModernButton
            {
                Text = "Server config",
                Variant = ButtonVariant.Ghost,
                Size = new Size(140, 34),
                Location = new Point(168, 15)
            };
            _clientTab.Click += (_, _) => ShowConfig(client: true);
            _serverTab.Click += (_, _) => ShowConfig(client: false);

            var copyButton = new ModernButton
            {
                Text = "Copy",
                Glyph = Glyphs.Copy,
                Variant = ButtonVariant.Standard,
                Size = new Size(110, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Top = 15
            };
            copyButton.Click += (_, _) => CopyCurrent();
            header.Resize += (_, _) => copyButton.Left = header.ClientSize.Width - 20 - copyButton.Width;

            var separator = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.Border };

            header.Controls.Add(_clientTab);
            header.Controls.Add(_serverTab);
            header.Controls.Add(copyButton);
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
                Font = new Font(Theme.PickMonoFontFamily(), 10F),
                Text = clientConfig
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 20), BackColor = Theme.Window };
            body.Controls.Add(_viewer);

            Controls.Add(body);
            Controls.Add(header);

            Shown += (_, _) => Theme.ApplyDarkScrollBars(_viewer);
        }

        private void ShowConfig(bool client)
        {
            _showingClient = client;
            _clientTab.Variant = client ? ButtonVariant.Accent : ButtonVariant.Ghost;
            _serverTab.Variant = client ? ButtonVariant.Ghost : ButtonVariant.Accent;
            _clientTab.Invalidate();
            _serverTab.Invalidate();

            _viewer.Text = client ? _clientConfig : _serverConfig;
            _viewer.SelectionStart = 0;
        }

        private void CopyCurrent()
        {
            var text = _showingClient ? _clientConfig : _serverConfig;
            if (string.IsNullOrEmpty(text))
                return;

            Clipboard.SetText(text);
        }
    }
}
