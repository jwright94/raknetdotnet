using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RakNetDotNet;

namespace RakNetDotNetSample
{
    class Apple
    {
        ServerNetworkIDGenerator gen;
        public Apple()
        {
            gen = new ServerNetworkIDGenerator();
            gen.Parent = this;
        }

        public void Func1(RPCParameters rpcParams)
        {
            Program.MainForm.ReceiveServerMessage("Apple Func1" + Environment.NewLine);
        }

        public NetworkID GetNetworkID()
        {
            return gen.GetNetworkID();
        }
    }

    class Server
    {
        #region Fields
        /// <summary>
        /// RakNet Server Interface
        /// </summary>
        RakPeerInterface server;

        /// <summary>
        /// Thread that will constantly poll for packets in the background
        /// </summary>
        Thread networkPoll;
        #endregion

        #region Constructor
        public Server()
        {
            server = RakNetworkFactory.GetRakPeerInterface();
            server.RegisterAsRemoteProcedureCall("ServerRPC", ServerRPC);
            server.RegisterClassMemberRPC("Apple_Func1", typeof(Apple).GetMethod("Func1"));
        }
        #endregion

        #region Methods
        #region Public
        public static void ServerRPC(RPCParameters rpcParameters)
        {
            Program.MainForm.ReceiveServerMessage("In serverRPC no input" + Environment.NewLine);
        }

        /// <summary>
        /// Starts the server, and start the networkPoll thread
        /// </summary>
        public void Start()
        {
            bool b = server.Startup(128, 61160, 30);
            server.SetMaximumIncomingConnections(128);
            if (b)
                Program.MainForm.ReceiveServerMessage("Server started." + Environment.NewLine);
            else
                throw new Exception("Error starting server.");

            this.networkPoll = new Thread(new ThreadStart(PollPackets));
            this.networkPoll.Start();
        }

        /// <summary>
        /// Stops the server and networkPoll thread
        /// </summary>
        public void Stop()
        {
            this.networkPoll.Abort();
            this.server.Shutdown(0);
        }

        /// <summary>
        /// Broadcasts a message
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            this.server.Send(data, data.Length, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetDotNet.RakNet.UNASSIGNED_SYSTEM_ADDRESS, true);
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
                p = this.server.Receive();
                while (p != null)
                {
                    Program.MainForm.ReceiveServerMessage(encoder.GetString(p.data, 0, (int)p.length) + Environment.NewLine);
                    this.server.DeallocatePacket(p);
                    p = this.server.Receive();
                }
                Thread.Sleep(30);
            }
        }
        #endregion
        #endregion
    }
}
