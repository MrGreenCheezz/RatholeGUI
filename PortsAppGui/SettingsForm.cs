namespace PortsAppGui
{
    public class SettingsForm : Form
    {
        private readonly ConfigStore _config;
        private readonly Action _save;
        private readonly Dictionary<string, TextBox> _fields = new();

        public SettingsForm(ConfigStore config, Action save)
        {
            _config = config;
            _save = save;

            Text = "RatholeGUI settings";
            Size = new Size(560, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 14,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddField(layout, "Server SSH host:port", nameof(config.ServerAddress), config.ServerAddress);
            AddField(layout, "Server username", nameof(config.ServerUsername), config.ServerUsername);
            AddField(layout, "Server password", nameof(config.ServerPassword), config.ServerPassword, true);
            AddField(layout, "Server rathole path", nameof(config.ServerRatholePath), config.ServerRatholePath);
            AddField(layout, "Server TOML path", nameof(config.ServerTomlPath), config.ServerTomlPath);
            AddField(layout, "Client SSH host:port", nameof(config.ClientAddress), config.ClientAddress);
            AddField(layout, "Client username", nameof(config.ClientUsername), config.ClientUsername);
            AddField(layout, "Client password", nameof(config.ClientPassword), config.ClientPassword, true);
            AddField(layout, "Client rathole path", nameof(config.ClientRatholePath), config.ClientRatholePath);
            AddField(layout, "Client TOML path", nameof(config.ClientTomlPath), config.ClientTomlPath);

            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var saveButton = new Button { Text = "Save", Width = 90 };
            var cancelButton = new Button { Text = "Cancel", Width = 90 };
            var testServerButton = new Button { Text = "Test server SSH", Width = 120 };
            var testClientButton = new Button { Text = "Test client SSH", Width = 120 };

            saveButton.Click += (_, _) => SaveAndClose();
            cancelButton.Click += (_, _) => Close();
            testServerButton.Click += (_, _) => TestConnection(isServer: true);
            testClientButton.Click += (_, _) => TestConnection(isServer: false);

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(testClientButton);
            buttonPanel.Controls.Add(testServerButton);

            layout.Controls.Add(buttonPanel, 0, layout.RowCount - 1);
            layout.SetColumnSpan(buttonPanel, 2);
            Controls.Add(layout);
        }

        private void AddField(TableLayoutPanel layout, string labelText, string key, string value, bool password = false)
        {
            var row = _fields.Count;
            var label = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var textBox = new TextBox { Text = value, Dock = DockStyle.Fill, UseSystemPasswordChar = password };
            _fields[key] = textBox;
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(textBox, 1, row);
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
                MessageBox.Show("Address must be in host:port format.", "Invalid address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var connector = isServer
                ? new SshConnector(host, port, _config.ServerUsername, _config.ServerPassword)
                : new SshConnector(host, port, _config.ClientUsername, _config.ClientPassword);

            var ok = connector.TestConnection(out var error);
            MessageBox.Show(ok ? "SSH connection OK." : error, ok ? "Success" : "SSH error", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
