namespace PortsAppGui
{
    partial class ServiceControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            ServiceNameTextBox = new TextBox();
            ServiceTokenTextBox = new TextBox();
            label2 = new Label();
            ServerAdressTextBox = new TextBox();
            label3 = new Label();
            ClientAdressTextBox = new TextBox();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            ClientPortTextBox = new TextBox();
            ServerPortTextBox = new TextBox();
            label7 = new Label();
            label8 = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 0;
            label1.Text = "Service name";
            // 
            // ServiceNameTextBox
            // 
            ServiceNameTextBox.Location = new Point(3, 18);
            ServiceNameTextBox.Name = "ServiceNameTextBox";
            ServiceNameTextBox.Size = new Size(163, 23);
            ServiceNameTextBox.TabIndex = 1;
            ServiceNameTextBox.TextChanged += textBox1_TextChanged;
            // 
            // ServiceTokenTextBox
            // 
            ServiceTokenTextBox.Location = new Point(3, 62);
            ServiceTokenTextBox.Name = "ServiceTokenTextBox";
            ServiceTokenTextBox.Size = new Size(163, 23);
            ServiceTokenTextBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 44);
            label2.Name = "label2";
            label2.Size = new Size(77, 15);
            label2.TabIndex = 2;
            label2.Text = "Service token";
            // 
            // ServerAdressTextBox
            // 
            ServerAdressTextBox.Location = new Point(198, 62);
            ServerAdressTextBox.Name = "ServerAdressTextBox";
            ServerAdressTextBox.Size = new Size(268, 23);
            ServerAdressTextBox.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(198, 44);
            label3.Name = "label3";
            label3.Size = new Size(75, 15);
            label3.TabIndex = 6;
            label3.Text = "Server adress";
            // 
            // ClientAdressTextBox
            // 
            ClientAdressTextBox.Location = new Point(198, 18);
            ClientAdressTextBox.Name = "ClientAdressTextBox";
            ClientAdressTextBox.Size = new Size(268, 23);
            ClientAdressTextBox.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(198, 0);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 4;
            label4.Text = "Client adress";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(472, 21);
            label5.Name = "label5";
            label5.Size = new Size(10, 15);
            label5.TabIndex = 8;
            label5.Text = ":";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(472, 65);
            label6.Name = "label6";
            label6.Size = new Size(10, 15);
            label6.TabIndex = 9;
            label6.Text = ":";
            // 
            // ClientPortTextBox
            // 
            ClientPortTextBox.Location = new Point(488, 18);
            ClientPortTextBox.Name = "ClientPortTextBox";
            ClientPortTextBox.Size = new Size(99, 23);
            ClientPortTextBox.TabIndex = 10;
            // 
            // ServerPortTextBox
            // 
            ServerPortTextBox.Location = new Point(488, 62);
            ServerPortTextBox.Name = "ServerPortTextBox";
            ServerPortTextBox.Size = new Size(99, 23);
            ServerPortTextBox.TabIndex = 11;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(488, 0);
            label7.Name = "label7";
            label7.Size = new Size(63, 15);
            label7.TabIndex = 12;
            label7.Text = "Client port";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(488, 44);
            label8.Name = "label8";
            label8.Size = new Size(64, 15);
            label8.TabIndex = 13;
            label8.Text = "Server port";
            // 
            // button1
            // 
            button1.Location = new Point(594, -1);
            button1.Name = "button1";
            button1.Size = new Size(22, 25);
            button1.TabIndex = 14;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ServiceControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(button1);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(ServerPortTextBox);
            Controls.Add(ClientPortTextBox);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(ServerAdressTextBox);
            Controls.Add(label3);
            Controls.Add(ClientAdressTextBox);
            Controls.Add(label4);
            Controls.Add(ServiceTokenTextBox);
            Controls.Add(label2);
            Controls.Add(ServiceNameTextBox);
            Controls.Add(label1);
            Name = "ServiceControl";
            Size = new Size(615, 98);
            Load += ServiceControl_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox ServiceTokenTextBox;
        private Label label2;
        private TextBox ServerAdressTextBox;
        private Label label3;
        private TextBox ClientAdressTextBox;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox ClientPortTextBox;
        private TextBox ServerPortTextBox;
        private Label label7;
        private Label label8;
        public TextBox ServiceNameTextBox;
        public Button button1;
    }
}
