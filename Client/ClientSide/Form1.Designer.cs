namespace ClientSide
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
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.textBox_Port = new System.Windows.Forms.TextBox();
            this.label_IP = new System.Windows.Forms.Label();
            this.label_Port = new System.Windows.Forms.Label();
            this.button_Connect = new System.Windows.Forms.Button();
            this.textBox_fileName = new System.Windows.Forms.TextBox();
            this.label_fileName = new System.Windows.Forms.Label();
            this.button_Browse = new System.Windows.Forms.Button();
            this.button_Send = new System.Windows.Forms.Button();
            this.button_Disconnect = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.button_Request = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_IP
            // 
            this.textBox_IP.Location = new System.Drawing.Point(95, 30);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.Size = new System.Drawing.Size(130, 22);
            this.textBox_IP.TabIndex = 0;
            // 
            // textBox_Port
            // 
            this.textBox_Port.Location = new System.Drawing.Point(95, 77);
            this.textBox_Port.Name = "textBox_Port";
            this.textBox_Port.Size = new System.Drawing.Size(130, 22);
            this.textBox_Port.TabIndex = 1;
            // 
            // label_IP
            // 
            this.label_IP.AutoSize = true;
            this.label_IP.Location = new System.Drawing.Point(36, 33);
            this.label_IP.Name = "label_IP";
            this.label_IP.Size = new System.Drawing.Size(28, 17);
            this.label_IP.TabIndex = 2;
            this.label_IP.Text = "IP: ";
            // 
            // label_Port
            // 
            this.label_Port.AutoSize = true;
            this.label_Port.Location = new System.Drawing.Point(22, 80);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(42, 17);
            this.label_Port.TabIndex = 3;
            this.label_Port.Text = "Port: ";
            // 
            // button_Connect
            // 
            this.button_Connect.BackColor = System.Drawing.SystemColors.Control;
            this.button_Connect.Location = new System.Drawing.Point(25, 118);
            this.button_Connect.Name = "button_Connect";
            this.button_Connect.Size = new System.Drawing.Size(200, 44);
            this.button_Connect.TabIndex = 4;
            this.button_Connect.Text = "CONNECT";
            this.button_Connect.UseVisualStyleBackColor = false;
            this.button_Connect.Click += new System.EventHandler(this.button_Connect_Click);
            // 
            // textBox_fileName
            // 
            this.textBox_fileName.Enabled = false;
            this.textBox_fileName.Location = new System.Drawing.Point(95, 244);
            this.textBox_fileName.Name = "textBox_fileName";
            this.textBox_fileName.Size = new System.Drawing.Size(130, 22);
            this.textBox_fileName.TabIndex = 5;
            // 
            // label_fileName
            // 
            this.label_fileName.AutoSize = true;
            this.label_fileName.Location = new System.Drawing.Point(10, 244);
            this.label_fileName.Name = "label_fileName";
            this.label_fileName.Size = new System.Drawing.Size(71, 17);
            this.label_fileName.TabIndex = 6;
            this.label_fileName.Text = "File Path: ";
            // 
            // button_Browse
            // 
            this.button_Browse.Enabled = false;
            this.button_Browse.Location = new System.Drawing.Point(95, 283);
            this.button_Browse.Name = "button_Browse";
            this.button_Browse.Size = new System.Drawing.Size(130, 33);
            this.button_Browse.TabIndex = 7;
            this.button_Browse.Text = "Browse";
            this.button_Browse.UseVisualStyleBackColor = true;
            this.button_Browse.Click += new System.EventHandler(this.button_Browse_Click);
            // 
            // button_Send
            // 
            this.button_Send.Enabled = false;
            this.button_Send.Location = new System.Drawing.Point(95, 335);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(130, 33);
            this.button_Send.TabIndex = 8;
            this.button_Send.Text = "Send";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.button_Send_Click);
            // 
            // button_Disconnect
            // 
            this.button_Disconnect.BackColor = System.Drawing.SystemColors.Control;
            this.button_Disconnect.Enabled = false;
            this.button_Disconnect.Location = new System.Drawing.Point(25, 175);
            this.button_Disconnect.Name = "button_Disconnect";
            this.button_Disconnect.Size = new System.Drawing.Size(200, 43);
            this.button_Disconnect.TabIndex = 9;
            this.button_Disconnect.Text = "DISCONNECT";
            this.button_Disconnect.UseVisualStyleBackColor = false;
            this.button_Disconnect.Click += new System.EventHandler(this.button_Disconnect_Click);
            // 
            // richTextBox
            // 
            this.richTextBox.Enabled = false;
            this.richTextBox.Location = new System.Drawing.Point(259, 30);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(509, 663);
            this.richTextBox.TabIndex = 10;
            this.richTextBox.Text = "";
            // 
            // button_Request
            // 
            this.button_Request.Enabled = false;
            this.button_Request.Location = new System.Drawing.Point(95, 388);
            this.button_Request.Name = "button_Request";
            this.button_Request.Size = new System.Drawing.Size(130, 33);
            this.button_Request.TabIndex = 11;
            this.button_Request.Text = "Request";
            this.button_Request.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 705);
            this.Controls.Add(this.button_Request);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.button_Disconnect);
            this.Controls.Add(this.button_Send);
            this.Controls.Add(this.button_Browse);
            this.Controls.Add(this.label_fileName);
            this.Controls.Add(this.textBox_fileName);
            this.Controls.Add(this.button_Connect);
            this.Controls.Add(this.label_Port);
            this.Controls.Add(this.label_IP);
            this.Controls.Add(this.textBox_Port);
            this.Controls.Add(this.textBox_IP);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.TextBox textBox_Port;
        private System.Windows.Forms.Label label_IP;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.Button button_Connect;
        private System.Windows.Forms.TextBox textBox_fileName;
        private System.Windows.Forms.Label label_fileName;
        private System.Windows.Forms.Button button_Browse;
        private System.Windows.Forms.Button button_Send;
        private System.Windows.Forms.Button button_Disconnect;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button button_Request;
    }
}

