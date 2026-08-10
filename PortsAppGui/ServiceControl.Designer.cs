using PortsAppGui.UI;

namespace PortsAppGui
{
    partial class ServiceControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Tips = new ToolTip(components);

            NameCaption = new Label();
            ServiceNameTextBox = new ModernTextBox();
            TokenCaption = new Label();
            ServiceTokenTextBox = new ModernTextBox();
            RegenerateTokenButton = new ModernButton();
            EnabledCheckBox = new ModernCheckBox();
            NoDelayCheckBox = new ModernCheckBox();
            UdpCheckBox = new ModernCheckBox();
            LocalCaption = new Label();
            ClientAddressTextBox = new ModernTextBox();
            ClientSeparator = new Label();
            ClientPortTextBox = new ModernTextBox();
            ScanButton = new ModernButton();
            PublicCaption = new Label();
            ServerAddressTextBox = new ModernTextBox();
            ServerSeparator = new Label();
            ServerPortTextBox = new ModernTextBox();
            DeleteButton = new ModernButton();
            SuspendLayout();
            //
            // NameCaption
            //
            NameCaption.AutoSize = true;
            NameCaption.Font = Theme.Caption;
            NameCaption.ForeColor = Theme.TextMuted;
            NameCaption.Location = new Point(18, 14);
            NameCaption.Name = "NameCaption";
            NameCaption.Text = "SERVICE NAME";
            //
            // ServiceNameTextBox
            //
            ServiceNameTextBox.Location = new Point(18, 32);
            ServiceNameTextBox.Name = "ServiceNameTextBox";
            ServiceNameTextBox.PlaceholderText = "my-app";
            ServiceNameTextBox.Size = new Size(190, 32);
            ServiceNameTextBox.TabIndex = 0;
            //
            // TokenCaption
            //
            TokenCaption.AutoSize = true;
            TokenCaption.Font = Theme.Caption;
            TokenCaption.ForeColor = Theme.TextMuted;
            TokenCaption.Location = new Point(216, 14);
            TokenCaption.Name = "TokenCaption";
            TokenCaption.Text = "TOKEN";
            //
            // ServiceTokenTextBox
            //
            ServiceTokenTextBox.Location = new Point(216, 32);
            ServiceTokenTextBox.Name = "ServiceTokenTextBox";
            ServiceTokenTextBox.PlaceholderText = "shared secret";
            ServiceTokenTextBox.Size = new Size(224, 32);
            ServiceTokenTextBox.TabIndex = 1;
            //
            // RegenerateTokenButton
            //
            RegenerateTokenButton.Glyph = Glyphs.Refresh;
            RegenerateTokenButton.Location = new Point(446, 32);
            RegenerateTokenButton.Name = "RegenerateTokenButton";
            RegenerateTokenButton.Size = new Size(32, 32);
            RegenerateTokenButton.TabIndex = 2;
            RegenerateTokenButton.Text = "";
            RegenerateTokenButton.Variant = ButtonVariant.Ghost;
            RegenerateTokenButton.Click += RegenerateTokenButton_Click;
            //
            // EnabledCheckBox
            //
            EnabledCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EnabledCheckBox.AutoWidth = false;
            EnabledCheckBox.Checked = true;
            EnabledCheckBox.Location = new Point(582, 36);
            EnabledCheckBox.Name = "EnabledCheckBox";
            EnabledCheckBox.Size = new Size(82, 24);
            EnabledCheckBox.TabIndex = 8;
            EnabledCheckBox.Text = "Enabled";
            //
            // NoDelayCheckBox
            //
            NoDelayCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NoDelayCheckBox.AutoWidth = false;
            NoDelayCheckBox.Location = new Point(678, 36);
            NoDelayCheckBox.Name = "NoDelayCheckBox";
            NoDelayCheckBox.Size = new Size(88, 24);
            NoDelayCheckBox.TabIndex = 9;
            NoDelayCheckBox.Text = "NoDelay";
            //
            // UdpCheckBox
            //
            UdpCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            UdpCheckBox.AutoWidth = false;
            UdpCheckBox.Location = new Point(780, 36);
            UdpCheckBox.Name = "UdpCheckBox";
            UdpCheckBox.Size = new Size(62, 24);
            UdpCheckBox.TabIndex = 10;
            UdpCheckBox.Text = "UDP";
            //
            // LocalCaption
            //
            LocalCaption.AutoSize = true;
            LocalCaption.Font = Theme.Caption;
            LocalCaption.ForeColor = Theme.TextMuted;
            LocalCaption.Location = new Point(18, 76);
            LocalCaption.Name = "LocalCaption";
            LocalCaption.Text = "LOCAL APP (CLIENT SIDE)";
            //
            // ClientAddressTextBox
            //
            ClientAddressTextBox.Location = new Point(18, 94);
            ClientAddressTextBox.Name = "ClientAddressTextBox";
            ClientAddressTextBox.PlaceholderText = "127.0.0.1";
            ClientAddressTextBox.Size = new Size(170, 32);
            ClientAddressTextBox.TabIndex = 3;
            //
            // ClientSeparator
            //
            ClientSeparator.AutoSize = true;
            ClientSeparator.ForeColor = Theme.TextMuted;
            ClientSeparator.Location = new Point(192, 102);
            ClientSeparator.Name = "ClientSeparator";
            ClientSeparator.Text = ":";
            //
            // ClientPortTextBox
            //
            ClientPortTextBox.Location = new Point(204, 94);
            ClientPortTextBox.Name = "ClientPortTextBox";
            ClientPortTextBox.PlaceholderText = "port";
            ClientPortTextBox.Size = new Size(72, 32);
            ClientPortTextBox.TabIndex = 4;
            //
            // ScanButton
            //
            ScanButton.Glyph = Glyphs.Search;
            ScanButton.Location = new Point(286, 94);
            ScanButton.Name = "ScanButton";
            ScanButton.Size = new Size(110, 32);
            ScanButton.TabIndex = 5;
            ScanButton.Text = "From app";
            ScanButton.Variant = ButtonVariant.Standard;
            ScanButton.Click += ScanButton_Click;
            //
            // PublicCaption
            //
            PublicCaption.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PublicCaption.AutoSize = true;
            PublicCaption.Font = Theme.Caption;
            PublicCaption.ForeColor = Theme.TextMuted;
            PublicCaption.Location = new Point(544, 76);
            PublicCaption.Name = "PublicCaption";
            PublicCaption.Text = "PUBLIC ENDPOINT (SERVER SIDE)";
            //
            // ServerAddressTextBox
            //
            ServerAddressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ServerAddressTextBox.Location = new Point(544, 94);
            ServerAddressTextBox.Name = "ServerAddressTextBox";
            ServerAddressTextBox.PlaceholderText = "0.0.0.0";
            ServerAddressTextBox.Size = new Size(160, 32);
            ServerAddressTextBox.TabIndex = 6;
            //
            // ServerSeparator
            //
            ServerSeparator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ServerSeparator.AutoSize = true;
            ServerSeparator.ForeColor = Theme.TextMuted;
            ServerSeparator.Location = new Point(710, 102);
            ServerSeparator.Name = "ServerSeparator";
            ServerSeparator.Text = ":";
            //
            // ServerPortTextBox
            //
            ServerPortTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ServerPortTextBox.Location = new Point(722, 94);
            ServerPortTextBox.Name = "ServerPortTextBox";
            ServerPortTextBox.PlaceholderText = "port";
            ServerPortTextBox.Size = new Size(72, 32);
            ServerPortTextBox.TabIndex = 7;
            //
            // DeleteButton
            //
            DeleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteButton.Glyph = Glyphs.Delete;
            DeleteButton.Location = new Point(810, 94);
            DeleteButton.Name = "DeleteButton";
            DeleteButton.Size = new Size(32, 32);
            DeleteButton.TabIndex = 11;
            DeleteButton.Text = "";
            DeleteButton.Variant = ButtonVariant.Ghost;
            DeleteButton.Click += DeleteButton_Click;
            //
            // ServiceControl
            //
            BackColor = Theme.Card;
            CornerRadius = 10;
            FillColor = Theme.Card;
            BorderColor = Theme.Border;
            StripeWidth = 4;
            StripeColor = Theme.Accent;
            Controls.Add(NameCaption);
            Controls.Add(ServiceNameTextBox);
            Controls.Add(TokenCaption);
            Controls.Add(ServiceTokenTextBox);
            Controls.Add(RegenerateTokenButton);
            Controls.Add(EnabledCheckBox);
            Controls.Add(NoDelayCheckBox);
            Controls.Add(UdpCheckBox);
            Controls.Add(LocalCaption);
            Controls.Add(ClientAddressTextBox);
            Controls.Add(ClientSeparator);
            Controls.Add(ClientPortTextBox);
            Controls.Add(ScanButton);
            Controls.Add(PublicCaption);
            Controls.Add(ServerAddressTextBox);
            Controls.Add(ServerSeparator);
            Controls.Add(ServerPortTextBox);
            Controls.Add(DeleteButton);
            Margin = new Padding(0, 0, 0, 10);
            MinimumSize = new Size(780, 148);
            Name = "ServiceControl";
            Size = new Size(860, 148);

            Tips.SetToolTip(RegenerateTokenButton, "Generate a random token");
            Tips.SetToolTip(ScanButton, "Pick a listening application on this PC");
            Tips.SetToolTip(DeleteButton, "Remove this service");
            Tips.SetToolTip(NoDelayCheckBox, "Disable Nagle's algorithm (lower latency)");
            Tips.SetToolTip(UdpCheckBox, "Also forward the same port over UDP");

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolTip Tips;
        private Label NameCaption;
        private Label TokenCaption;
        private Label LocalCaption;
        private Label PublicCaption;
        private Label ClientSeparator;
        private Label ServerSeparator;
        private ModernTextBox ServiceTokenTextBox;
        private ModernTextBox ServerAddressTextBox;
        private ModernTextBox ClientAddressTextBox;
        private ModernTextBox ClientPortTextBox;
        private ModernTextBox ServerPortTextBox;
        private ModernButton RegenerateTokenButton;
        private ModernButton ScanButton;
        public ModernTextBox ServiceNameTextBox;
        public ModernButton DeleteButton;
        public ModernCheckBox NoDelayCheckBox;
        public ModernCheckBox UdpCheckBox;
        public ModernCheckBox EnabledCheckBox;
    }
}
