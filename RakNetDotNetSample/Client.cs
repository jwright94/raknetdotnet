using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RakNetDotNet;

namespace RakNetDotNetSample
{
    class Client
    {
        #region Fields
        /// <summary>
        /// RakNet.NET Client Interface
        /// </summary>
        RakPeerInterface client;

        /// <summary>
        /// Thread to constantly poll for packets
        /// </summary>
        Thread networkPoll;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public Client()
        {
            client = RakNetworkFactory.GetRakPeerInterface();
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Connects the client, starts polling for packets
        /// </summary>
        public void Connect()
        {
            client.Startup(1, 30, new SocketDescriptor(), 1);
            if (this.client.Connect("127.0.0.1", 61160, null, 0))
                Program.MainForm.ReceiveClientMessage("Connected." + Environment.NewLine);
            else
                throw new Exception("Unable to connect.");

            this.networkPoll = new Thread(new ThreadStart(PollPackets));
            this.networkPoll.Start();
        }

        /// <summary>
        /// Disconnects the client and aborts the networkPoll thread
        /// </summary>
        public void Disconnect()
        {
            this.networkPoll.Abort();
            this.client.Shutdown(0);
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message">Message</param>
        public void SendMessage(string message)
        {
            if (message.StartsWith("serverrpc"))
            {
                client.RPC("ServerRPC", new byte[0], 0, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE, 0, RakNetDotNet.RakNet.UNASSIGNED_SYSTEM_ADDRESS, true, 0, RakNetDotNet.RakNet.UNASSIGNED_NETWORK_ID, null);
            }
            else if (message.StartsWith("objectmemberrpc"))
            {
                client.RPC("Apple_Func1", new byte[0], 0, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetDotNet.RakNet.UNASSIGNED_SYSTEM_ADDRESS, true, 0, Program.apple.GetNetworkID(), null);
            }
            else
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetDotNet.RakNet.UNASSIGNED_SYSTEM_ADDRESS, true);
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// Constantly polls for packets and acts on them
        /// You could implement callbacks and that jazz, but this is just a sample
        /// </summary>
        private void PollPackets()
        {
            Packet p;
            Encoding encoder = new UnicodeEncoding();
            while (true)
            {
                p = client.Receive();
                while (p != null)
                {
                    Program.MainForm.ReceiveClientMessage(encoder.GetString(p.data, 0, (int)p.length) + Environment.NewLine);
                    client.DeallocatePacket(p);
                    p = client.Receive();
                }
                Thread.Sleep(30);
            }
        }
        #endregion
        #endregion
    }
}
