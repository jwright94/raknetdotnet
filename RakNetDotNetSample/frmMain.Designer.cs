namespace RakNetDotNetSample
{
    partial class frmMain
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.pageServer = new System.Windows.Forms.TabPage();
            this.btnServerStop = new System.Windows.Forms.Button();
            this.btnServerStart = new System.Windows.Forms.Button();
            this.btnServerSend = new System.Windows.Forms.Button();
            this.txtServerMessage = new System.Windows.Forms.TextBox();
            this.txtServerChat = new System.Windows.Forms.RichTextBox();
            this.pageClient = new System.Windows.Forms.TabPage();
            this.btnClientDisconnect = new System.Windows.Forms.Button();
            this.btnClientConnect = new System.Windows.Forms.Button();
            this.btnClientSend = new System.Windows.Forms.Button();
            this.txtClientMessage = new System.Windows.Forms.TextBox();
            this.txtClientChat = new System.Windows.Forms.RichTextBox();
            this.tabMain.SuspendLayout();
            this.pageServer.SuspendLayout();
            this.pageClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.pageServer);
            this.tabMain.Controls.Add(this.pageClient);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(474, 273);
            this.tabMain.TabIndex = 0;
            // 
            // pageServer
            // 
            this.pageServer.Controls.Add(this.btnServerStop);
            this.pageServer.Controls.Add(this.btnServerStart);
            this.pageServer.Controls.Add(this.btnServerSend);
            this.pageServer.Controls.Add(this.txtServerMessage);
            this.pageServer.Controls.Add(this.txtServerChat);
            this.pageServer.Location = new System.Drawing.Point(4, 22);
            this.pageServer.Name = "pageServer";
            this.pageServer.Padding = new System.Windows.Forms.Padding(3);
            this.pageServer.Size = new System.Drawing.Size(466, 247);
            this.pageServer.TabIndex = 0;
            this.pageServer.Text = "Server";
            this.pageServer.UseVisualStyleBackColor = true;
            // 
            // btnServerStop
            // 
            this.btnServerStop.Enabled = false;
            this.btnServerStop.Location = new System.Drawing.Point(87, 216);
            this.btnServerStop.Name = "btnServerStop";
            this.btnServerStop.Size = new System.Drawing.Size(75, 23);
            this.btnServerStop.TabIndex = 4;
            this.btnServerStop.Text = "Stop";
            this.btnServerStop.UseVisualStyleBackColor = true;
            this.btnServerStop.Click += new System.EventHandler(this.btnServerStop_Click);
            // 
            // btnServerStart
            // 
            this.btnServerStart.Location = new System.Drawing.Point(6, 216);
            this.btnServerStart.Name = "btnServerStart";
            this.btnServerStart.Size = new System.Drawing.Size(75, 23);
            this.btnServerStart.TabIndex = 3;
            this.btnServerStart.Text = "Start";
            this.btnServerStart.UseVisualStyleBackColor = true;
            this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
            // 
            // btnServerSend
            // 
            this.btnServerSend.Location = new System.Drawing.Point(383, 187);
            this.btnServerSend.Name = "btnServerSend";
            this.btnServerSend.Size = new System.Drawing.Size(75, 23);
            this.btnServerSend.TabIndex = 2;
            this.btnServerSend.Text = "&Send";
            this.btnServerSend.UseVisualStyleBackColor = true;
            this.btnServerSend.Click += new System.EventHandler(this.btnServerSend_Click);
            // 
            // txtServerMessage
            // 
            this.txtServerMessage.Location = new System.Drawing.Point(6, 189);
            this.txtServerMessage.Name = "txtServerMessage";
            this.txtServerMessage.Size = new System.Drawing.Size(370, 21);
            this.txtServerMessage.TabIndex = 1;
            // 
            // txtServerChat
            // 
            this.txtServerChat.Location = new System.Drawing.Point(6, 6);
            this.txtServerChat.Name = "txtServerChat";
            this.txtServerChat.Size = new System.Drawing.Size(452, 175);
            this.txtServerChat.TabIndex = 0;
            this.txtServerChat.Text = "";
            // 
            // pageClient
            // 
            this.pageClient.Controls.Add(this.btnClientDisconnect);
            this.pageClient.Controls.Add(this.btnClientConnect);
            this.pageClient.Controls.Add(this.btnClientSend);
            this.pageClient.Controls.Add(this.txtClientMessage);
            this.pageClient.Controls.Add(this.txtClientChat);
            this.pageClient.Location = new System.Drawing.Point(4, 22);
            this.pageClient.Name = "pageClient";
            this.pageClient.Padding = new System.Windows.Forms.Padding(3);
            this.pageClient.Size = new System.Drawing.Size(466, 247);
            this.pageClient.TabIndex = 1;
            this.pageClient.Text = "Client";
            this.pageClient.UseVisualStyleBackColor = true;
            // 
            // btnClientDisconnect
            // 
            this.btnClientDisconnect.Enabled = false;
            this.btnClientDisconnect.Location = new System.Drawing.Point(88, 217);
            this.btnClientDisconnect.Name = "btnClientDisconnect";
            this.btnClientDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnClientDisconnect.TabIndex = 9;
            this.btnClientDisconnect.Text = "Disconnect";
            this.btnClientDisconnect.UseVisualStyleBackColor = true;
            this.btnClientDisconnect.Click += new System.EventHandler(this.btnClientDisconnect_Click);
            // 
            // btnClientConnect
            // 
            this.btnClientConnect.Location = new System.Drawing.Point(7, 217);
            this.btnClientConnect.Name = "btnClientConnect";
            this.btnClientConnect.Size = new System.Drawing.Size(75, 23);
            this.btnClientConnect.TabIndex = 8;
            this.btnClientConnect.Text = "Connect";
            this.btnClientConnect.UseVisualStyleBackColor = true;
            this.btnClientConnect.Click += new System.EventHandler(this.btnClientConnect_Click);
            // 
            // btnClientSend
            // 
            this.btnClientSend.Location = new System.Drawing.Point(384, 188);
            this.btnClientSend.Name = "btnClientSend";
            this.btnClientSend.Size = new System.Drawing.Size(75, 23);
            this.btnClientSend.TabIndex = 7;
            this.btnClientSend.Text = "&Send";
            this.btnClientSend.UseVisualStyleBackColor = true;
            this.btnClientSend.Click += new System.EventHandler(this.btnClientSend_Click);
            // 
            // txtClientMessage
            // 
            this.txtClientMessage.Location = new System.Drawing.Point(7, 190);
            this.txtClientMessage.Name = "txtClientMessage";
            this.txtClientMessage.Size = new System.Drawing.Size(370, 21);
            this.txtClientMessage.TabIndex = 6;
            // 
            // txtClientChat
            // 
            this.txtClientChat.Location = new System.Drawing.Point(7, 7);
            this.txtClientChat.Name = "txtClientChat";
            this.txtClientChat.Size = new System.Drawing.Size(452, 175);
            this.txtClientChat.TabIndex = 5;
            this.txtClientChat.Text = "";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 273);
            this.Controls.Add(this.tabMain);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmMain";
            this.Text = "RakNet.NET Sample";
            this.tabMain.ResumeLayout(false);
            this.pageServer.ResumeLayout(false);
            this.pageServer.PerformLayout();
            this.pageClient.ResumeLayout(false);
            this.pageClient.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage pageServer;
        private System.Windows.Forms.TabPage pageClient;
        private System.Windows.Forms.Button btnServerStop;
        private System.Windows.Forms.Button btnServerStart;
        private System.Windows.Forms.Button btnServerSend;
        private System.Windows.Forms.TextBox txtServerMessage;
        private System.Windows.Forms.RichTextBox txtServerChat;
        private System.Windows.Forms.Button btnClientDisconnect;
        private System.Windows.Forms.Button btnClientConnect;
        private System.Windows.Forms.Button btnClientSend;
        private System.Windows.Forms.TextBox txtClientMessage;
        private System.Windows.Forms.RichTextBox txtClientChat;
    }
}

