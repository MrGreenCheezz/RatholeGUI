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
            label1 = new Label();
            ClientPathTextBox = new TextBox();
            label2 = new Label();
            ServerPathTextBox = new TextBox();
            AddRuleButton = new Button();
            SaveRulesButton = new Button();
            RunButton = new Button();
            panel1 = new FlowLayoutPanel();
            StopButton = new Button();
            ErrorText = new Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(112, 15);
            label1.TabIndex = 0;
            label1.Text = "Client toml file path";
            // 
            // ClientPathTextBox
            // 
            ClientPathTextBox.Location = new Point(12, 27);
            ClientPathTextBox.Name = "ClientPathTextBox";
            ClientPathTextBox.Size = new Size(267, 23);
            ClientPathTextBox.TabIndex = 1;
            ClientPathTextBox.Text = "/here/path/to/file";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 66);
            label2.Name = "label2";
            label2.Size = new Size(113, 15);
            label2.TabIndex = 2;
            label2.Text = "Server toml file path";
            // 
            // ServerPathTextBox
            // 
            ServerPathTextBox.Location = new Point(12, 84);
            ServerPathTextBox.Name = "ServerPathTextBox";
            ServerPathTextBox.Size = new Size(267, 23);
            ServerPathTextBox.TabIndex = 3;
            ServerPathTextBox.Text = "/here/path/to/file";
            // 
            // AddRuleButton
            // 
            AddRuleButton.Location = new Point(12, 434);
            AddRuleButton.Name = "AddRuleButton";
            AddRuleButton.Size = new Size(112, 23);
            AddRuleButton.TabIndex = 5;
            AddRuleButton.Text = "Add rule";
            AddRuleButton.UseVisualStyleBackColor = true;
            AddRuleButton.Click += AddRuleButton_Click;
            // 
            // SaveRulesButton
            // 
            SaveRulesButton.Location = new Point(130, 434);
            SaveRulesButton.Name = "SaveRulesButton";
            SaveRulesButton.Size = new Size(112, 23);
            SaveRulesButton.TabIndex = 6;
            SaveRulesButton.Text = "Save config";
            SaveRulesButton.UseVisualStyleBackColor = true;
            SaveRulesButton.Click += SaveRulesButton_Click;
            // 
            // RunButton
            // 
            RunButton.Location = new Point(524, 27);
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
            panel1.Size = new Size(670, 282);
            panel1.TabIndex = 9;
            // 
            // StopButton
            // 
            StopButton.Enabled = false;
            StopButton.Location = new Point(524, 83);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(129, 23);
            StopButton.TabIndex = 10;
            StopButton.Text = "Stop connection!";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(694, 469);
            Controls.Add(StopButton);
            Controls.Add(panel1);
            Controls.Add(RunButton);
            Controls.Add(SaveRulesButton);
            Controls.Add(AddRuleButton);
            Controls.Add(ServerPathTextBox);
            Controls.Add(label2);
            Controls.Add(ClientPathTextBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox ClientPathTextBox;
        private Label label2;
        private TextBox ServerPathTextBox;
        private Button AddRuleButton;
        private Button SaveRulesButton;
        private Button RunButton;
        private FlowLayoutPanel panel1;
        private Button StopButton;
        private Label ErrorText;
    }
}
