using PortsAppGui.UI;

namespace PortsAppGui
{
    partial class Form1
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Tips = new ToolTip(components);

            HeaderPanel = new Panel();
            LogoBox = new PictureBox();
            TitleLabel = new Label();
            SubtitleLabel = new Label();
            ConnectionPill = new StatusPill();
            HeaderSeparator = new Panel();

            ToolbarPanel = new Panel();
            RunButton = new ModernButton();
            StopButton = new ModernButton();
            SettingsButton = new ModernButton();
            TestSshButton = new ModernButton();
            PreviewTomlButton = new ModernButton();
            LogsButton = new ModernButton();
            ToolbarSeparator = new Panel();

            ContentPanel = new Panel();
            SectionPanel = new Panel();
            ServicesLabel = new Label();
            ServicesCountLabel = new Label();
            AddRuleButton = new ModernButton();
            AddFromAppButton = new ModernButton();
            panel1 = new FlowLayoutPanel();

            HintCard = new CardPanel();
            HintTitle = new Label();
            HintText = new Label();
            HintButton = new ModernButton();
            EmptyStateLabel = new Label();

            FooterPanel = new Panel();
            StatusLabel = new Label();
            SaveRulesButton = new ModernButton();
            FooterSeparator = new Panel();

            ((System.ComponentModel.ISupportInitialize)LogoBox).BeginInit();
            HeaderPanel.SuspendLayout();
            ToolbarPanel.SuspendLayout();
            ContentPanel.SuspendLayout();
            SectionPanel.SuspendLayout();
            panel1.SuspendLayout();
            HintCard.SuspendLayout();
            FooterPanel.SuspendLayout();
            SuspendLayout();

            // Anchor offsets are captured against the parent's size at ResumeLayout time,
            // so every container that hosts right-anchored children must already have its
            // final docked width here.
            HeaderPanel.Size = new Size(1000, 72);
            ToolbarPanel.Size = new Size(1000, 64);
            SectionPanel.Size = new Size(952, 54);
            FooterPanel.Size = new Size(1000, 64);

            // ------------------------------------------------------------------ header
            //
            // LogoBox
            //
            LogoBox.Location = new Point(24, 18);
            LogoBox.Name = "LogoBox";
            LogoBox.Size = new Size(36, 36);
            LogoBox.SizeMode = PictureBoxSizeMode.Zoom;
            LogoBox.TabStop = false;
            //
            // TitleLabel
            //
            TitleLabel.AutoSize = true;
            TitleLabel.Font = Theme.Title;
            TitleLabel.ForeColor = Theme.Text;
            TitleLabel.Location = new Point(72, 15);
            TitleLabel.Name = "TitleLabel";
            TitleLabel.Text = "RatholeGUI";
            //
            // SubtitleLabel
            //
            SubtitleLabel.AutoSize = true;
            SubtitleLabel.Font = Theme.Caption;
            SubtitleLabel.ForeColor = Theme.TextMuted;
            SubtitleLabel.Location = new Point(74, 41);
            SubtitleLabel.Name = "SubtitleLabel";
            SubtitleLabel.Text = "Reverse proxy tunnels over rathole";
            //
            // ConnectionPill
            //
            // Repositioned by Form1.PositionStatusPill: the pill resizes itself to its text,
            // which fights a Right anchor.
            ConnectionPill.Kind = StatusKind.Unknown;
            ConnectionPill.Location = new Point(700, 21);
            ConnectionPill.Name = "ConnectionPill";
            ConnectionPill.Size = new Size(276, 30);
            ConnectionPill.Text = "Starting…";
            //
            // HeaderSeparator
            //
            HeaderSeparator.BackColor = Theme.Border;
            HeaderSeparator.Dock = DockStyle.Bottom;
            HeaderSeparator.Height = 1;
            HeaderSeparator.Name = "HeaderSeparator";
            //
            // HeaderPanel
            //
            HeaderPanel.BackColor = Theme.Surface;
            HeaderPanel.Controls.Add(LogoBox);
            HeaderPanel.Controls.Add(TitleLabel);
            HeaderPanel.Controls.Add(SubtitleLabel);
            HeaderPanel.Controls.Add(ConnectionPill);
            HeaderPanel.Controls.Add(HeaderSeparator);
            HeaderPanel.Dock = DockStyle.Top;
            HeaderPanel.Height = 72;
            HeaderPanel.Name = "HeaderPanel";

            // ----------------------------------------------------------------- toolbar
            //
            // RunButton
            //
            RunButton.Glyph = Glyphs.Play;
            RunButton.Location = new Point(24, 14);
            RunButton.Name = "RunButton";
            RunButton.Size = new Size(160, 36);
            RunButton.TabIndex = 0;
            RunButton.Text = "Run connection";
            RunButton.Variant = ButtonVariant.Accent;
            RunButton.Click += RunButton_Click;
            //
            // StopButton
            //
            StopButton.Enabled = false;
            StopButton.Glyph = Glyphs.Stop;
            StopButton.Location = new Point(192, 14);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(150, 36);
            StopButton.TabIndex = 1;
            StopButton.Text = "Stop connection";
            StopButton.Variant = ButtonVariant.Danger;
            StopButton.Click += StopButton_Click;
            //
            // SettingsButton
            //
            SettingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SettingsButton.Glyph = Glyphs.Settings;
            SettingsButton.Location = new Point(472, 14);
            SettingsButton.Name = "SettingsButton";
            SettingsButton.Size = new Size(120, 36);
            SettingsButton.TabIndex = 2;
            SettingsButton.Text = "Settings";
            SettingsButton.Variant = ButtonVariant.Ghost;
            SettingsButton.Click += SettingsButton_Click;
            //
            // TestSshButton
            //
            TestSshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TestSshButton.Glyph = Glyphs.Network;
            TestSshButton.Location = new Point(600, 14);
            TestSshButton.Name = "TestSshButton";
            TestSshButton.Size = new Size(120, 36);
            TestSshButton.TabIndex = 3;
            TestSshButton.Text = "Test SSH";
            TestSshButton.Variant = ButtonVariant.Ghost;
            TestSshButton.Click += TestSshButton_Click;
            //
            // PreviewTomlButton
            //
            PreviewTomlButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PreviewTomlButton.Glyph = Glyphs.Page;
            PreviewTomlButton.Location = new Point(728, 14);
            PreviewTomlButton.Name = "PreviewTomlButton";
            PreviewTomlButton.Size = new Size(140, 36);
            PreviewTomlButton.TabIndex = 4;
            PreviewTomlButton.Text = "Preview TOML";
            PreviewTomlButton.Variant = ButtonVariant.Ghost;
            PreviewTomlButton.Click += PreviewTomlButton_Click;
            //
            // LogsButton
            //
            LogsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LogsButton.Glyph = Glyphs.Document;
            LogsButton.Location = new Point(876, 14);
            LogsButton.Name = "LogsButton";
            LogsButton.Size = new Size(100, 36);
            LogsButton.TabIndex = 5;
            LogsButton.Text = "Logs";
            LogsButton.Variant = ButtonVariant.Ghost;
            LogsButton.Click += LogsButton_Click;
            //
            // ToolbarSeparator
            //
            ToolbarSeparator.BackColor = Theme.Border;
            ToolbarSeparator.Dock = DockStyle.Bottom;
            ToolbarSeparator.Height = 1;
            ToolbarSeparator.Name = "ToolbarSeparator";
            //
            // ToolbarPanel
            //
            ToolbarPanel.BackColor = Theme.Window;
            ToolbarPanel.Controls.Add(RunButton);
            ToolbarPanel.Controls.Add(StopButton);
            ToolbarPanel.Controls.Add(SettingsButton);
            ToolbarPanel.Controls.Add(TestSshButton);
            ToolbarPanel.Controls.Add(PreviewTomlButton);
            ToolbarPanel.Controls.Add(LogsButton);
            ToolbarPanel.Controls.Add(ToolbarSeparator);
            ToolbarPanel.Dock = DockStyle.Top;
            ToolbarPanel.Height = 64;
            ToolbarPanel.Name = "ToolbarPanel";

            // ------------------------------------------------------------- section head
            //
            // ServicesLabel
            //
            ServicesLabel.AutoSize = true;
            ServicesLabel.Font = Theme.Section;
            ServicesLabel.ForeColor = Theme.Text;
            ServicesLabel.Location = new Point(0, 14);
            ServicesLabel.Name = "ServicesLabel";
            ServicesLabel.Text = "Services";
            //
            // ServicesCountLabel
            //
            ServicesCountLabel.AutoSize = true;
            ServicesCountLabel.Font = Theme.Body;
            ServicesCountLabel.ForeColor = Theme.TextMuted;
            ServicesCountLabel.Location = new Point(72, 16);
            ServicesCountLabel.Name = "ServicesCountLabel";
            ServicesCountLabel.Text = "";
            //
            // AddFromAppButton
            //
            AddFromAppButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddFromAppButton.Glyph = Glyphs.Search;
            AddFromAppButton.Location = new Point(634, 10);
            AddFromAppButton.Name = "AddFromAppButton";
            AddFromAppButton.Size = new Size(170, 34);
            AddFromAppButton.TabIndex = 6;
            AddFromAppButton.Text = "Add from running app";
            AddFromAppButton.Variant = ButtonVariant.Standard;
            AddFromAppButton.Click += AddFromAppButton_Click;
            //
            // AddRuleButton
            //
            AddRuleButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddRuleButton.Glyph = Glyphs.Add;
            AddRuleButton.Location = new Point(812, 10);
            AddRuleButton.Name = "AddRuleButton";
            AddRuleButton.Size = new Size(140, 34);
            AddRuleButton.TabIndex = 7;
            AddRuleButton.Text = "Add manually";
            AddRuleButton.Variant = ButtonVariant.Standard;
            AddRuleButton.Click += AddRuleButton_Click;
            //
            // SectionPanel
            //
            SectionPanel.BackColor = Theme.Window;
            SectionPanel.Controls.Add(ServicesLabel);
            SectionPanel.Controls.Add(ServicesCountLabel);
            SectionPanel.Controls.Add(AddFromAppButton);
            SectionPanel.Controls.Add(AddRuleButton);
            SectionPanel.Dock = DockStyle.Top;
            SectionPanel.Height = 54;
            SectionPanel.Name = "SectionPanel";

            // ---------------------------------------------------------------- hint card
            //
            // HintTitle
            //
            HintTitle.AutoSize = true;
            HintTitle.Font = Theme.BodySemibold;
            HintTitle.ForeColor = Theme.Text;
            HintTitle.Location = new Point(20, 18);
            HintTitle.Name = "HintTitle";
            HintTitle.Text = "Finish the setup";
            //
            // HintText
            //
            HintText.AutoSize = false;
            HintText.Font = Theme.Body;
            HintText.ForeColor = Theme.TextMuted;
            HintText.Location = new Point(20, 40);
            HintText.Name = "HintText";
            HintText.Size = new Size(600, 40);
            HintText.Text = "Add the SSH address, user and rathole directory for both machines before starting a tunnel.";
            //
            // HintButton
            //
            HintButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HintButton.Glyph = Glyphs.Settings;
            HintButton.Location = new Point(780, 30);
            HintButton.Name = "HintButton";
            HintButton.Size = new Size(150, 36);
            HintButton.Text = "Open settings";
            HintButton.Variant = ButtonVariant.Accent;
            HintButton.Click += SettingsButton_Click;
            //
            // HintCard
            //
            HintCard.BackColor = Theme.Mix(Theme.Card, Theme.Accent, 0.07);
            HintCard.BorderColor = Theme.Mix(Theme.Border, Theme.Accent, 0.45);
            HintCard.Controls.Add(HintTitle);
            HintCard.Controls.Add(HintText);
            HintCard.Controls.Add(HintButton);
            HintCard.CornerRadius = 10;
            HintCard.FillColor = Theme.Mix(Theme.Card, Theme.Accent, 0.07);
            HintCard.Margin = new Padding(0, 0, 0, 10);
            HintCard.Name = "HintCard";
            HintCard.Size = new Size(952, 96);
            HintCard.StripeColor = Theme.Accent;
            HintCard.StripeWidth = 4;
            HintCard.Visible = false;
            //
            // EmptyStateLabel
            //
            EmptyStateLabel.AutoSize = false;
            EmptyStateLabel.Font = Theme.Body;
            EmptyStateLabel.ForeColor = Theme.TextMuted;
            EmptyStateLabel.Margin = new Padding(4, 24, 0, 0);
            EmptyStateLabel.Name = "EmptyStateLabel";
            EmptyStateLabel.Size = new Size(600, 44);
            EmptyStateLabel.Text = "No services yet. Use \"Add from running app\" to pick a port from an application " +
                                   "that is already listening on this PC, or \"Add manually\" to type one in.";
            EmptyStateLabel.Visible = false;
            //
            // panel1
            //
            panel1.AutoScroll = true;
            panel1.BackColor = Theme.Window;
            panel1.Controls.Add(HintCard);
            panel1.Controls.Add(EmptyStateLabel);
            panel1.Dock = DockStyle.Fill;
            panel1.FlowDirection = FlowDirection.TopDown;
            panel1.Name = "panel1";
            panel1.Padding = new Padding(0, 0, 0, 16);
            panel1.WrapContents = false;
            //
            // ContentPanel
            //
            ContentPanel.BackColor = Theme.Window;
            ContentPanel.Controls.Add(panel1);
            ContentPanel.Controls.Add(SectionPanel);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Name = "ContentPanel";
            ContentPanel.Padding = new Padding(24, 0, 24, 0);

            // ------------------------------------------------------------------ footer
            //
            // StatusLabel
            //
            StatusLabel.Dock = DockStyle.Fill;
            StatusLabel.Font = Theme.Body;
            StatusLabel.ForeColor = Theme.TextMuted;
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Text = "Starting…";
            StatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // SaveRulesButton
            //
            SaveRulesButton.Dock = DockStyle.Right;
            SaveRulesButton.Glyph = Glyphs.Save;
            SaveRulesButton.Name = "SaveRulesButton";
            SaveRulesButton.Size = new Size(140, 34);
            SaveRulesButton.TabIndex = 8;
            SaveRulesButton.Text = "Save config";
            SaveRulesButton.Variant = ButtonVariant.Standard;
            SaveRulesButton.Click += SaveRulesButton_Click;
            //
            // FooterSeparator
            //
            FooterSeparator.BackColor = Theme.Border;
            FooterSeparator.Dock = DockStyle.Top;
            FooterSeparator.Height = 1;
            FooterSeparator.Name = "FooterSeparator";
            //
            // FooterPanel
            //
            FooterPanel.BackColor = Theme.Surface;
            FooterPanel.Controls.Add(StatusLabel);
            FooterPanel.Controls.Add(SaveRulesButton);
            FooterPanel.Controls.Add(FooterSeparator);
            FooterPanel.Dock = DockStyle.Bottom;
            FooterPanel.Height = 64;
            FooterPanel.Name = "FooterPanel";
            FooterPanel.Padding = new Padding(24, 15, 24, 15);

            // -------------------------------------------------------------------- form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Theme.Window;
            ClientSize = new Size(1000, 700);
            Controls.Add(ContentPanel);
            Controls.Add(FooterPanel);
            Controls.Add(ToolbarPanel);
            Controls.Add(HeaderPanel);
            Font = Theme.Body;
            ForeColor = Theme.Text;
            MinimumSize = new Size(956, 620);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RatholeGUI";
            FormClosing += Form1_FormClosing;

            Tips.SetToolTip(RunButton, "Upload the generated TOML files and start rathole on both machines");
            Tips.SetToolTip(StopButton, "Stop every rathole process on both machines");
            Tips.SetToolTip(AddFromAppButton, "Scan this PC for listening applications and create a service from one");
            Tips.SetToolTip(AddRuleButton, "Add an empty service and fill it in by hand");

            HintCard.ResumeLayout(false);
            HintCard.PerformLayout();
            panel1.ResumeLayout(false);
            SectionPanel.ResumeLayout(false);
            SectionPanel.PerformLayout();
            ContentPanel.ResumeLayout(false);
            ToolbarPanel.ResumeLayout(false);
            HeaderPanel.ResumeLayout(false);
            HeaderPanel.PerformLayout();
            FooterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LogoBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ToolTip Tips;

        private Panel HeaderPanel;
        private PictureBox LogoBox;
        private Label TitleLabel;
        private Label SubtitleLabel;
        private StatusPill ConnectionPill;
        private Panel HeaderSeparator;

        private Panel ToolbarPanel;
        private ModernButton RunButton;
        private ModernButton StopButton;
        private ModernButton SettingsButton;
        private ModernButton TestSshButton;
        private ModernButton PreviewTomlButton;
        private ModernButton LogsButton;
        private Panel ToolbarSeparator;

        private Panel ContentPanel;
        private Panel SectionPanel;
        private Label ServicesLabel;
        private Label ServicesCountLabel;
        private ModernButton AddRuleButton;
        private ModernButton AddFromAppButton;
        private FlowLayoutPanel panel1;

        private CardPanel HintCard;
        private Label HintTitle;
        private Label HintText;
        private ModernButton HintButton;
        private Label EmptyStateLabel;

        private Panel FooterPanel;
        private Label StatusLabel;
        private ModernButton SaveRulesButton;
        private Panel FooterSeparator;
    }
}
