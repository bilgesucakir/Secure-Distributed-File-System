namespace Server1_2
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listenPortNum = new System.Windows.Forms.TextBox();
            this.IpAdress = new System.Windows.Forms.TextBox();
            this.masterPortNum = new System.Windows.Forms.TextBox();
            this.Port = new System.Windows.Forms.Label();
            this.Ip = new System.Windows.Forms.Label();
            this.masterPort = new System.Windows.Forms.Label();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.listen_button = new System.Windows.Forms.Button();
            this.connect_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listenPortNum
            // 
            this.listenPortNum.Location = new System.Drawing.Point(90, 74);
            this.listenPortNum.Name = "listenPortNum";
            this.listenPortNum.Size = new System.Drawing.Size(100, 22);
            this.listenPortNum.TabIndex = 0;
            // 
            // IpAdress
            // 
            this.IpAdress.Location = new System.Drawing.Point(90, 229);
            this.IpAdress.Name = "IpAdress";
            this.IpAdress.Size = new System.Drawing.Size(100, 22);
            this.IpAdress.TabIndex = 1;
            // 
            // masterPortNum
            // 
            this.masterPortNum.Location = new System.Drawing.Point(90, 274);
            this.masterPortNum.Name = "masterPortNum";
            this.masterPortNum.Size = new System.Drawing.Size(100, 22);
            this.masterPortNum.TabIndex = 2;
            // 
            // Port
            // 
            this.Port.AutoSize = true;
            this.Port.Location = new System.Drawing.Point(34, 79);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(38, 17);
            this.Port.TabIndex = 3;
            this.Port.Text = "Port:";
            // 
            // Ip
            // 
            this.Ip.AutoSize = true;
            this.Ip.Location = new System.Drawing.Point(34, 234);
            this.Ip.Name = "Ip";
            this.Ip.Size = new System.Drawing.Size(23, 17);
            this.Ip.TabIndex = 4;
            this.Ip.Text = "Ip:";
            // 
            // masterPort
            // 
            this.masterPort.AutoSize = true;
            this.masterPort.Location = new System.Drawing.Point(34, 279);
            this.masterPort.Name = "masterPort";
            this.masterPort.Size = new System.Drawing.Size(38, 17);
            this.masterPort.TabIndex = 5;
            this.masterPort.Text = "Port:";
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(218, 36);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(770, 529);
            this.logs.TabIndex = 6;
            this.logs.Text = "";
            // 
            // listen_button
            // 
            this.listen_button.Location = new System.Drawing.Point(90, 120);
            this.listen_button.Name = "listen_button";
            this.listen_button.Size = new System.Drawing.Size(81, 23);
            this.listen_button.TabIndex = 7;
            this.listen_button.Text = "listen";
            this.listen_button.UseVisualStyleBackColor = true;
            this.listen_button.Click += new System.EventHandler(this.listen_button_Click);
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(90, 319);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(81, 23);
            this.connect_button.TabIndex = 8;
            this.connect_button.Text = "connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.connect_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(279, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 17);
            this.label1.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 577);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connect_button);
            this.Controls.Add(this.listen_button);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.masterPort);
            this.Controls.Add(this.Ip);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.masterPortNum);
            this.Controls.Add(this.IpAdress);
            this.Controls.Add(this.listenPortNum);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox listenPortNum;
        private System.Windows.Forms.TextBox IpAdress;
        private System.Windows.Forms.TextBox masterPortNum;
        private System.Windows.Forms.Label Port;
        private System.Windows.Forms.Label Ip;
        private System.Windows.Forms.Label masterPort;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Button listen_button;
        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.Label label1;
    }
}

