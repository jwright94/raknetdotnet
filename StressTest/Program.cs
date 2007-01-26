using System;
using System.Collections.Generic;
using System.Text;
using RakNetDotNet;

namespace StressTest
{
    class Program
    {
        static void Main(string[] args)
        {
            char ch;
            bool isServer;
            RakPeerInterface serverRakPeer = RakNetworkFactory.GetRakPeerInterface();
            List<RakPeerInterface> clientRakPeers = new List<RakPeerInterface>();
            int clustorSize = 16;
            int maxNumberOfClustors = 1024/clustorSize;
            Console.WriteLine("(S)erver or (C)lient?");
            ch = Console.ReadKey(true).KeyChar;
            if (ch == 's')
            {
                isServer = true;
                serverRakPeer.SetMaximumIncomingConnections(1024);
                SocketDescriptor socketDescriptor = new SocketDescriptor(12345, string.Empty);
                serverRakPeer.Startup(1024, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                Console.WriteLine("Server started");
            }
            else
            {
                isServer = false;
                Console.Write("Number of Clients: ");
                string numberOfClientsString = Console.ReadLine();
                int numberOfClients = Convert.ToInt32(numberOfClientsString);
                SocketDescriptor socketDescriptor = new SocketDescriptor();
                for (int i = 0; i < clustorSize * numberOfClients; ++i)
                {
                    RakPeerInterface clientRakPeer = RakNetworkFactory.GetRakPeerInterface();
                    clientRakPeer.Startup(1, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                    clientRakPeers.Add(clientRakPeer);
                }
                Console.WriteLine("Client started");
                Console.Write("Enter server IP: ");
                string serverIP = Console.ReadLine();
                Console.WriteLine("Connecting to server.");
                foreach (RakPeerInterface clientRakPeer in clientRakPeers)
                {
                    clientRakPeer.Connect(serverIP, 12345, string.Empty, 0);
                }
            }
            Console.WriteLine("(E)xit");
            while (true)
            {
                
                System.Threading.Thread.Sleep(0);
            }

            //TelnetTransport tt = new TelnetTransport();
            //RakPeerInterface rakPeer = RakNetworkFactory.GetRakPeerInterface();
            //TestCommandServer(tt, 23, rakPeer);
        }

        static void TestCommandServer(TransportInterface ti, ushort port, RakPeerInterface rakPeer)
        {
            ConsoleServer consoleServer = new ConsoleServer();
            RakNetCommandParser rcp = new RakNetCommandParser();
            LogCommandParser lcp = new LogCommandParser();
            uint lastlog = 0;
            IntPtr testChannel = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi("TestChannel");  // you must call FreeHGlobal

            consoleServer.AddCommandParser(rcp);
            consoleServer.AddCommandParser(lcp);
            consoleServer.SetTransportProvider(ti, port);
            rcp.SetRakPeerInterface(rakPeer);
            lcp.AddChannel(testChannel);
            while (true)
            {
                consoleServer.Update();

                if (RakNetDotNet.RakNet.GetTime() > lastlog + 4000)
                {
                    lcp.WriteLog(testChannel, "Test of logger");
                    lastlog = RakNetDotNet.RakNet.GetTime();
                }

                System.Threading.Thread.Sleep(30);
            }
        }
    }
}
