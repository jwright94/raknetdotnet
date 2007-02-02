using System;
using System.Collections.Generic;
using System.Text;
using RakNetDotNet;

namespace StressTest
{
    class Connection
    {
        public RakPeerInterface RakPeer;
        public bool ConnectionCompleted;
        public Connection(RakPeerInterface rakPeer, bool connectionCompleted)
        {
            RakPeer = rakPeer;
            ConnectionCompleted = connectionCompleted;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            char ch;
            bool isServer;
            RakPeerInterface serverRakPeer = RakNetworkFactory.GetRakPeerInterface();
            const int maxNumberOfClients = 2048;
            IList<Connection> clientConnections = new List<Connection>(maxNumberOfClients);
            const ushort portNum = 12345;
            int querySize = 1000;
            byte[] dummyData = new byte[querySize];
            Random r = new Random();
            r.NextBytes(dummyData);
            dummyData[0] = (byte)RakNetBindings.ID_USER_PACKET_ENUM;
            List<SystemAddress> incomingConnections = new List<SystemAddress>(maxNumberOfClients);
            Console.WriteLine("(S)erver or (C)lient?");
            ch = Console.ReadKey(true).KeyChar;
            if (ch == 's')
            {
                isServer = true;
                serverRakPeer.SetMaximumIncomingConnections(maxNumberOfClients);
                SocketDescriptor socketDescriptor = new SocketDescriptor(portNum, string.Empty);
                serverRakPeer.Startup(maxNumberOfClients, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                Console.WriteLine("Server started");
            }
            else
            {
                isServer = false;
                Console.Write("Number of Clients: ");
                string numberOfClientsString = Console.ReadLine();
                int numberOfClients = Convert.ToInt32(numberOfClientsString);
                Console.Write("Query size: ");
                string querySizeString = Console.ReadLine();
                querySize = Convert.ToInt32(querySizeString);
                SocketDescriptor socketDescriptor = new SocketDescriptor();
                for (int i = 0; i < numberOfClients; ++i)
                {
                    RakPeerInterface clientRakPeer = RakNetworkFactory.GetRakPeerInterface();
                    clientRakPeer.Startup(1, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                    clientConnections.Add(new Connection(clientRakPeer, false));
                }
                Console.WriteLine("Client started");
                Console.Write("Enter server IP: ");
                string serverIP = Console.ReadLine();
                Console.WriteLine("Connecting to server.");
                foreach (Connection clientConnection in clientConnections)
                {
                    clientConnection.RakPeer.Connect(serverIP, portNum, string.Empty, 0);
                }
            }
            //Console.WriteLine("(E)xit");
            uint lastLog = 0;
            while (true)
            {
                if (isServer)
                {
                    Packet p = serverRakPeer.Receive();
                    if (p != null)
                    {
                        BitStream inBitStream = new BitStream(p, false);
                        byte packetIdentifier;
                        inBitStream.Read(out packetIdentifier);
                        switch (packetIdentifier)
                        {
                            case RakNetBindings.ID_NEW_INCOMING_CONNECTION:
                                Console.WriteLine("ID_NEW_INCOMING_CONNECTION");
                                //incomingConnections.Add(p.systemAddress);
                                break;
                            case RakNetBindings.ID_USER_PACKET_ENUM:
                                serverRakPeer.Send(inBitStream, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, p.systemAddress, false);
                                break;
                        }
                        serverRakPeer.DeallocatePacket(p);
                    }

                    SystemAddress systemAddress = serverRakPeer.GetSystemAddressFromIndex(0);
                    //if (systemAddress.binaryAddress != RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS.binaryAddress &&
                    //    systemAddress.port != RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS.port)
                    if(!systemAddress.Equals(RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS))
                    {
                        RakNetStatisticsStruct rss = serverRakPeer.GetStatistics(systemAddress);
                        if (RakNetBindings.GetTime() > lastLog + 4000)
                        {
                            Console.WriteLine("Packets sent:\t\t\t\t{0:D}", rss.packetsSent);
                            lastLog = RakNetBindings.GetTime();
                        }
                    }
                }
                else
                {
                    foreach(Connection clientConnection in clientConnections)
                    {
                        Packet p = clientConnection.RakPeer.Receive();
                        if (p != null)
                        {
                            BitStream inBitStream = new BitStream(p, false);
                            byte packetIdentifier;
                            inBitStream.Read(out packetIdentifier);
                            switch (packetIdentifier)
                            {
                                case RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED:
                                    Console.WriteLine("ID_CONNECTION_REQUEST_ACCEPTED");
                                    clientConnection.ConnectionCompleted = true;
                                    break;
                            }
                            clientConnection.RakPeer.DeallocatePacket(p);
                        }

                        if (clientConnection.ConnectionCompleted)
                        {
                            clientConnection.RakPeer.Send(dummyData, querySize, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                    }
                }
                System.Threading.Thread.Sleep(0);
            }

            //foreach (RakPeerInterface clientRakPeer in clientRakPeers)
            //{
            //    RakNetworkFactory.DestroyRakPeerInterface(clientRakPeer);
            //}
            //RakNetworkFactory.DestroyRakPeerInterface(serverRakPeer);
        }


    }
}
