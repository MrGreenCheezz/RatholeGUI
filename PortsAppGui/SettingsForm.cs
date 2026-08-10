using PortsAppGui.UI;

namespace PortsAppGui
{
    public class SettingsForm : Form
    {
        private const int CardWidth = 424;
        private const int LabelWidth = 132;
        private const int FieldWidth = 252;
        private const int RowHeight = 46;

        private readonly ConfigStore _config;
        private readonly Action _save;
        private readonly Dictionary<string, ModernTextBox> _fields = new();

        public SettingsForm(ConfigStore config, Action save)
        {
            _config = config;
            _save = save;

            Text = "RatholeGUI settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Icon = Theme.LoadAppIcon();
            Theme.ApplyTo(this);

            var serverCard = BuildSideCard(
                "Server machine",
                "The public box that accepts incoming traffic.",
                new Point(24, 64),
                new[]
                {
                    ("SSH host:port", nameof(config.ServerAddress), config.ServerAddress, false, "1.2.3.4:22"),
                    ("Username", nameof(config.ServerUsername), config.ServerUsername, false, "root"),
                    ("Password", nameof(config.ServerPassword), config.ServerPassword, true, ""),
                    ("rathole directory", nameof(config.ServerRatholePath), config.ServerRatholePath, false, "/opt/rathole/"),
                    ("Local TOML path", nameof(config.ServerTomlPath), config.ServerTomlPath, false, "server.toml")
                },
                isServer: true);

            var clientCard = BuildSideCard(
                "Client machine",
                "Where the tunnelled applications actually run.",
                new Point(24 + CardWidth + 20, 64),
                new[]
                {
                    ("SSH host:port", nameof(config.ClientAddress), config.ClientAddress, false, "127.0.0.1:22"),
                    ("Username", nameof(config.ClientUsername), config.ClientUsername, false, "user"),
                    ("Password", nameof(config.ClientPassword), config.ClientPassword, true, ""),
                    ("rathole directory", nameof(config.ClientRatholePath), config.ClientRatholePath, false, "/home/user/rathole/"),
                    ("Local TOML path", nameof(config.ClientTomlPath), config.ClientTomlPath, false, "client.toml")
                },
                isServer: false);

            var title = new Label
            {
                Text = "Connection settings",
                Font = Theme.Section,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(24, 24)
            };

            var warning = new Label
            {
                Text = "Credentials are stored as plain text in data.json — keep that file out of git.",
                Font = Theme.Caption,
                ForeColor = Theme.Warning,
                AutoSize = true,
                Location = new Point(24, serverCard.Bottom + 22)
            };

            var saveButton = new ModernButton
            {
                Text = "Save",
                Variant = ButtonVariant.Accent,
                Size = new Size(110, 36)
            };
            var cancelButton = new ModernButton
            {
                Text = "Cancel",
                Variant = ButtonVariant.Standard,
                Size = new Size(110, 36),
                DialogResult = DialogResult.Cancel
            };

            saveButton.Click += (_, _) => SaveAndClose();

            var contentWidth = 24 + CardWidth + 20 + CardWidth + 24;
            ClientSize = new Size(contentWidth, serverCard.Bottom + 22 + 46);

            saveButton.Location = new Point(ClientSize.Width - 24 - saveButton.Width, serverCard.Bottom + 16);
            cancelButton.Location = new Point(saveButton.Left - 10 - cancelButton.Width, serverCard.Bottom + 16);

            Controls.Add(title);
            Controls.Add(serverCard);
            Controls.Add(clientCard);
            Controls.Add(warning);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private CardPanel BuildSideCard(string title, string subtitle, Point location,
            (string Label, string Key, string Value, bool Password, string Placeholder)[] rows, bool isServer)
        {
            var card = new CardPanel
            {
                Location = location,
                Size = new Size(CardWidth, 66 + rows.Length * RowHeight + 54),
                CornerRadius = 10,
                StripeWidth = 3,
                StripeColor = isServer ? Theme.Accent : Theme.Success
            };

            card.Controls.Add(new Label
            {
                Text = title,
                Font = Theme.BodySemibold,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(18, 16)
            });

            card.Controls.Add(new Label
            {
                Text = subtitle,
                Font = Theme.Caption,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(18, 36)
            });

            var top = 66;
            foreach (var row in rows)
            {
                card.Controls.Add(new Label
                {
                    Text = row.Label,
                    Font = Theme.Body,
                    ForeColor = Theme.TextMuted,
                    AutoSize = false,
                    Size = new Size(LabelWidth, 32),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(18, top)
                });

                var field = new ModernTextBox
                {
                    Text = row.Value,
                    PlaceholderText = row.Placeholder,
                    UseSystemPasswordChar = row.Password,
                    Location = new Point(18 + LabelWidth, top),
                    Size = new Size(FieldWidth, 32)
                };

                _fields[row.Key] = field;
                card.Controls.Add(field);
                top += RowHeight;
            }

            var testButton = new ModernButton
            {
                Text = "Test SSH connection",
                Glyph = Glyphs.Network,
                Variant = ButtonVariant.Standard,
                Size = new Size(CardWidth - 36, 34),
                Location = new Point(18, top + 4)
            };
            testButton.Click += (_, _) => TestConnection(isServer);
            card.Controls.Add(testButton);

            return card;
        }

        private void SaveFields()
        {
            _config.ServerAddress = _fields[nameof(_config.ServerAddress)].Text;
            _config.ServerUsername = _fields[nameof(_config.ServerUsername)].Text;
            _config.ServerPassword = _fields[nameof(_config.ServerPassword)].Text;
            _config.ServerRatholePath = _fields[nameof(_config.ServerRatholePath)].Text;
            _config.ServerTomlPath = _fields[nameof(_config.ServerTomlPath)].Text;
            _config.ClientAddress = _fields[nameof(_config.ClientAddress)].Text;
            _config.ClientUsername = _fields[nameof(_config.ClientUsername)].Text;
            _config.ClientPassword = _fields[nameof(_config.ClientPassword)].Text;
            _config.ClientRatholePath = _fields[nameof(_config.ClientRatholePath)].Text;
            _config.ClientTomlPath = _fields[nameof(_config.ClientTomlPath)].Text;
        }

        private void SaveAndClose()
        {
            SaveFields();
            _save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TestConnection(bool isServer)
        {
            SaveFields();
            var address = isServer ? _config.ServerAddress : _config.ClientAddress;
            if (!ConfigValidator.TryParseHostPort(address, out var host, out var port))
            {
                Dialogs.Error(this, "Invalid address", "Address must be in host:port format, for example 10.0.0.5:22.");
                return;
            }

            var connector = isServer
                ? new SshConnector(host, port, _config.ServerUsername, _config.ServerPassword)
                : new SshConnector(host, port, _config.ClientUsername, _config.ClientPassword);

            var previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            bool ok;
            string error;
            try
            {
                ok = connector.TestConnection(out error);
            }
            finally
            {
                Cursor = previousCursor;
            }

            if (ok)
                Dialogs.Success(this, "SSH connection OK", $"Connected to {host}:{port} successfully.");
            else
                Dialogs.Error(this, "SSH error", error);
        }
    }
}
