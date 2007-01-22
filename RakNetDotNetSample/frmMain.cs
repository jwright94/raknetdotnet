using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RakNetDotNetSample
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private delegate void ReceiveMessageCallback(string message);
        private delegate void AddPlayerCallback(string name);
        private delegate void RemovePlayerCallback(string name);

        #region Server Events
        private void btnServerStart_Click(object sender, EventArgs e)
        {
            Program.DotNETServer.Start();
            btnServerStart.Enabled = false;
            btnServerStop.Enabled = true;
        }

        public void ReceiveServerMessage(string message)
        {
            if (txtServerChat.InvokeRequired)
            {
                ReceiveMessageCallback d = new ReceiveMessageCallback(ReceiveServerMessage);
                this.Invoke(d, new object[] { message });
            }
            else
                txtServerChat.Text += message;
        }

        private void btnServerSend_Click(object sender, EventArgs e)
        {
            if (txtServerMessage.Text.Length > 0)
            {
                Program.DotNETServer.SendMessage(txtServerMessage.Text);
                ReceiveServerMessage(txtServerMessage.Text + Environment.NewLine);
                txtServerMessage.Text = String.Empty;
            }
        }

        private void btnServerStop_Click(object sender, EventArgs e)
        {
            Program.DotNETServer.Stop();
            btnServerStart.Enabled = true;
            btnServerStop.Enabled = false;
        }
        #endregion

        #region Client Events
        private void btnClientConnect_Click(object sender, EventArgs e)
        {
            Program.DotNETClient.Connect();
            btnClientDisconnect.Enabled = true;
            btnClientConnect.Enabled = false;
        }

        public void ReceiveClientMessage(string message)
        {
            if (txtClientChat.InvokeRequired)
            {
                ReceiveMessageCallback d = new ReceiveMessageCallback(ReceiveClientMessage);
                this.Invoke(d, new object[] { message });
            }
            else
                txtClientChat.Text += message;
        }

        private void btnClientSend_Click(object sender, EventArgs e)
        {
            if (txtClientMessage.Text.Length > 0)
            {
                Program.DotNETClient.SendMessage(txtClientMessage.Text);
                ReceiveClientMessage(txtClientMessage.Text + Environment.NewLine);
                txtClientMessage.Text = String.Empty;
            }
        }

        private void btnClientDisconnect_Click(object sender, EventArgs e)
        {
            Program.DotNETClient.Disconnect();
            btnClientConnect.Enabled = true;
            btnClientDisconnect.Enabled = false;
        }
        #endregion
    }
}