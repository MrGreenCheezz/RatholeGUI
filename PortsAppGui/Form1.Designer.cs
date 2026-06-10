namespace PortsAppGui
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            AddRuleButton = new Button();
            SaveRulesButton = new Button();
            RunButton = new Button();
            panel1 = new FlowLayoutPanel();
            ErrorText = new Label();
            StopButton = new Button();
            pictureBox1 = new PictureBox();
            StatusLabel = new Label();
            SettingsButton = new Button();
            TestSshButton = new Button();
            PreviewTomlButton = new Button();
            LogsButton = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // AddRuleButton
            // 
            AddRuleButton.Location = new Point(539, 71);
            AddRuleButton.Name = "AddRuleButton";
            AddRuleButton.Size = new Size(129, 23);
            AddRuleButton.TabIndex = 5;
            AddRuleButton.Text = "Add rule";
            AddRuleButton.UseVisualStyleBackColor = true;
            AddRuleButton.Click += AddRuleButton_Click;
            // 
            // SaveRulesButton
            // 
            SaveRulesButton.Location = new Point(539, 100);
            SaveRulesButton.Name = "SaveRulesButton";
            SaveRulesButton.Size = new Size(129, 23);
            SaveRulesButton.TabIndex = 6;
            SaveRulesButton.Text = "Save config";
            SaveRulesButton.UseVisualStyleBackColor = true;
            SaveRulesButton.Click += SaveRulesButton_Click;
            // 
            // RunButton
            // 
            RunButton.Location = new Point(539, 13);
            RunButton.Name = "RunButton";
            RunButton.Size = new Size(129, 23);
            RunButton.TabIndex = 8;
            RunButton.Text = "Run connection!";
            RunButton.UseVisualStyleBackColor = true;
            RunButton.Click += RunButton_Click;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(ErrorText);
            panel1.Location = new Point(12, 137);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(15, 5, 0, 0);
            panel1.Size = new Size(656, 320);
            panel1.TabIndex = 9;
            // 
            // ErrorText
            // 
            ErrorText.AutoSize = true;
            ErrorText.Font = new Font("Segoe UI", 30F);
            ErrorText.Location = new Point(18, 5);
            ErrorText.Name = "ErrorText";
            ErrorText.Size = new Size(244, 54);
            ErrorText.TabIndex = 0;
            ErrorText.Text = "Fill data.json";
            ErrorText.Visible = false;
            // 
            // StopButton
            // 
            StopButton.Enabled = false;
            StopButton.Location = new Point(539, 42);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(129, 23);
            StopButton.TabIndex = 10;
            StopButton.Text = "Stop connection!";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(28, 14);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(80, 80);
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            // 
            // StatusLabel
            // 
            StatusLabel.AutoSize = true;
            StatusLabel.Location = new Point(28, 108);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(96, 15);
            StatusLabel.TabIndex = 12;
            StatusLabel.Text = "Status: Unknown";
            // 
            // SettingsButton
            // 
            SettingsButton.Location = new Point(390, 13);
            SettingsButton.Name = "SettingsButton";
            SettingsButton.Size = new Size(129, 23);
            SettingsButton.TabIndex = 13;
            SettingsButton.Text = "Settings";
            SettingsButton.UseVisualStyleBackColor = true;
            SettingsButton.Click += SettingsButton_Click;
            // 
            // TestSshButton
            // 
            TestSshButton.Location = new Point(390, 42);
            TestSshButton.Name = "TestSshButton";
            TestSshButton.Size = new Size(129, 23);
            TestSshButton.TabIndex = 14;
            TestSshButton.Text = "Test SSH";
            TestSshButton.UseVisualStyleBackColor = true;
            TestSshButton.Click += TestSshButton_Click;
            // 
            // PreviewTomlButton
            // 
            PreviewTomlButton.Location = new Point(390, 71);
            PreviewTomlButton.Name = "PreviewTomlButton";
            PreviewTomlButton.Size = new Size(129, 23);
            PreviewTomlButton.TabIndex = 15;
            PreviewTomlButton.Text = "Preview TOML";
            PreviewTomlButton.UseVisualStyleBackColor = true;
            PreviewTomlButton.Click += PreviewTomlButton_Click;
            // 
            // LogsButton
            // 
            LogsButton.Location = new Point(390, 100);
            LogsButton.Name = "LogsButton";
            LogsButton.Size = new Size(129, 23);
            LogsButton.TabIndex = 16;
            LogsButton.Text = "Logs";
            LogsButton.UseVisualStyleBackColor = true;
            LogsButton.Click += LogsButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 469);
            Controls.Add(LogsButton);
            Controls.Add(PreviewTomlButton);
            Controls.Add(TestSshButton);
            Controls.Add(SettingsButton);
            Controls.Add(StatusLabel);
            Controls.Add(pictureBox1);
            Controls.Add(StopButton);
            Controls.Add(panel1);
            Controls.Add(RunButton);
            Controls.Add(SaveRulesButton);
            Controls.Add(AddRuleButton);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "RatholeGUI_App";
            FormClosing += Form1_FormClosing;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button AddRuleButton;
        private Button SaveRulesButton;
        private Button RunButton;
        private FlowLayoutPanel panel1;
        private Button StopButton;
        private Label ErrorText;
        private PictureBox pictureBox1;
        private Label StatusLabel;
        private Button SettingsButton;
        private Button TestSshButton;
        private Button PreviewTomlButton;
        private Button LogsButton;
    }
}
