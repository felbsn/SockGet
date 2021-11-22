
namespace WinformClient
{
    partial class ClientForm
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
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.sendPanel = new System.Windows.Forms.Panel();
            this.tbMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.connectionPanel = new System.Windows.Forms.Panel();
            this.btnTagTest = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.tbAuthFile = new System.Windows.Forms.TextBox();
            this.btnSelectAuth = new System.Windows.Forms.Button();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.tbConnect = new System.Windows.Forms.Button();
            this.tbAddress = new System.Windows.Forms.TextBox();
            this.logPanel = new WinformClient.LogPanel();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.sendPanel.SuspendLayout();
            this.connectionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // sendPanel
            // 
            this.sendPanel.Controls.Add(this.tbMessage);
            this.sendPanel.Controls.Add(this.btnSend);
            this.sendPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sendPanel.Location = new System.Drawing.Point(0, 374);
            this.sendPanel.Name = "sendPanel";
            this.sendPanel.Size = new System.Drawing.Size(800, 76);
            this.sendPanel.TabIndex = 1;
            // 
            // tbMessage
            // 
            this.tbMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMessage.Location = new System.Drawing.Point(0, 0);
            this.tbMessage.Multiline = true;
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.Size = new System.Drawing.Size(707, 76);
            this.tbMessage.TabIndex = 9;
            this.tbMessage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbMessage_KeyPress);
            // 
            // btnSend
            // 
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.Location = new System.Drawing.Point(707, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(93, 76);
            this.btnSend.TabIndex = 10;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // connectionPanel
            // 
            this.connectionPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.connectionPanel.Controls.Add(this.btnSendFile);
            this.connectionPanel.Controls.Add(this.btnTagTest);
            this.connectionPanel.Controls.Add(this.lblName);
            this.connectionPanel.Controls.Add(this.tbAuthFile);
            this.connectionPanel.Controls.Add(this.btnSelectAuth);
            this.connectionPanel.Controls.Add(this.tbPort);
            this.connectionPanel.Controls.Add(this.tbConnect);
            this.connectionPanel.Controls.Add(this.tbAddress);
            this.connectionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.connectionPanel.Location = new System.Drawing.Point(0, 0);
            this.connectionPanel.Name = "connectionPanel";
            this.connectionPanel.Size = new System.Drawing.Size(800, 32);
            this.connectionPanel.TabIndex = 2;
            // 
            // btnTagTest
            // 
            this.btnTagTest.Location = new System.Drawing.Point(490, 4);
            this.btnTagTest.Name = "btnTagTest";
            this.btnTagTest.Size = new System.Drawing.Size(61, 23);
            this.btnTagTest.TabIndex = 16;
            this.btnTagTest.Text = "Tag Test";
            this.btnTagTest.UseVisualStyleBackColor = true;
            this.btnTagTest.Click += new System.EventHandler(this.btnTagTest_Click);
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(312, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(49, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "lblName";
            // 
            // tbAuthFile
            // 
            this.tbAuthFile.Location = new System.Drawing.Point(667, 5);
            this.tbAuthFile.Name = "tbAuthFile";
            this.tbAuthFile.Size = new System.Drawing.Size(130, 22);
            this.tbAuthFile.TabIndex = 5;
            // 
            // btnSelectAuth
            // 
            this.btnSelectAuth.Location = new System.Drawing.Point(557, 4);
            this.btnSelectAuth.Name = "btnSelectAuth";
            this.btnSelectAuth.Size = new System.Drawing.Size(104, 23);
            this.btnSelectAuth.TabIndex = 15;
            this.btnSelectAuth.Text = "Select Auth File";
            this.btnSelectAuth.UseVisualStyleBackColor = true;
            this.btnSelectAuth.Click += new System.EventHandler(this.btnSelectAuth_Click);
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(173, 5);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(52, 22);
            this.tbPort.TabIndex = 6;
            this.tbPort.Text = "9999";
            // 
            // tbConnect
            // 
            this.tbConnect.Location = new System.Drawing.Point(231, 4);
            this.tbConnect.Name = "tbConnect";
            this.tbConnect.Size = new System.Drawing.Size(75, 23);
            this.tbConnect.TabIndex = 8;
            this.tbConnect.Text = "Connect";
            this.tbConnect.UseVisualStyleBackColor = true;
            this.tbConnect.Click += new System.EventHandler(this.tbConnect_Click);
            // 
            // tbAddress
            // 
            this.tbAddress.Location = new System.Drawing.Point(3, 5);
            this.tbAddress.Name = "tbAddress";
            this.tbAddress.Size = new System.Drawing.Size(164, 22);
            this.tbAddress.TabIndex = 5;
            // 
            // logPanel
            // 
            this.logPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logPanel.Location = new System.Drawing.Point(0, 32);
            this.logPanel.Name = "logPanel";
            this.logPanel.Size = new System.Drawing.Size(800, 342);
            this.logPanel.TabIndex = 0;
            // 
            // btnSendFile
            // 
            this.btnSendFile.Location = new System.Drawing.Point(380, 4);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(104, 23);
            this.btnSendFile.TabIndex = 17;
            this.btnSendFile.Text = "Send File";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.logPanel);
            this.Controls.Add(this.connectionPanel);
            this.Controls.Add(this.sendPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ClientForm";
            this.Text = "Client";
            this.sendPanel.ResumeLayout(false);
            this.sendPanel.PerformLayout();
            this.connectionPanel.ResumeLayout(false);
            this.connectionPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog1;
        private LogPanel logPanel;
        private System.Windows.Forms.Panel sendPanel;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbMessage;
        private System.Windows.Forms.Panel connectionPanel;
        private System.Windows.Forms.TextBox tbAuthFile;
        private System.Windows.Forms.Button btnSelectAuth;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Button tbConnect;
        private System.Windows.Forms.TextBox tbAddress;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnTagTest;
        private System.Windows.Forms.Button btnSendFile;
    }
}

