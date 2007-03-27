using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RakNetDotNet;

namespace EventSystem
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("(S)erver or (U)nifiedNetwork or (C)lient?");
            char key = Console.ReadKey(true).KeyChar;
            if (key == 's' || key == 'S')
                ServerMain(args);
            else if (key == 'u' || key == 'U')
                UnifiedNetworkMain(args);
            else
                ClientMain(args);
        }

        private static void ClientMain(string[] args)
        {
            // TODO - parse options
            ushort clientPort = 20000;
            string serverIP = "127.0.0.1";

            GameManager game = new GameManager();
            IntroState intro = new IntroState();
            PlayState play = new PlayState(clientPort, serverIP);

            try
            {
                game.Start(intro);

                Console.WriteLine("All done!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured: {0}", e.ToString());
            }

            game.Dispose();
            intro.Dispose();
            play.Dispose();

            Console.WriteLine("Quiting...");
        }

        private static void ServerMain(string[] args)
        {
            EventCenterServer server = new EventCenterServer("server.xml");
            SampleEventFactory factory = ServiceConfigurator.Resolve<SampleEventFactory>();
            factory.Reset();
            RpcCalls rpcCalls = ServiceConfigurator.Resolve<RpcCalls>();
            rpcCalls.Reset();
            rpcCalls.ProcessEventOnServerSide += server.ProcessEvent;

            server.Start();

            factory.Reset();
            server.Dispose();
        }

        #region Unified Network

        private static void UnifiedNetworkMain(string[] args)
        {
            UnifiedNetwork network = ServiceConfigurator.Resolve<UnifiedNetwork>();
            int c = 0;
#if false
            const ushort NAME_SERVICE_PORT = 6000;
            bool isNS;

            Console.WriteLine("Server Port ? (NS=6000)");
            string input = Console.ReadLine();
            if (input.Equals(string.Empty))
            {
                isNS = true;
                serverPort = NAME_SERVICE_PORT;
            }
            else
            {
                isNS = false;
                serverPort = ushort.Parse(input);
            }

            Dictionary<string, object> extendedProperties = new Dictionary<string, object>();
            extendedProperties.Add("isNS", isNS);
            extendedProperties.Add("allowedPlayers", (ushort) 10);
            extendedProperties.Add("port", serverPort);
            UnifiedNetwork unifiedNetwork = new UnifiedNetwork("server.xml", extendedProperties);
            SampleEventFactory factory = ServiceConfigurator.Resolve<SampleEventFactory>();
            factory.Reset();
            RpcCalls rpcCalls = ServiceConfigurator.Resolve<RpcCalls>();
            rpcCalls.Reset();
            rpcCalls.ProcessEventOnServerSide += unifiedNetwork.ProcessEvent;

            if (!isNS)
            {
                unifiedNetwork.ConnectPlayer("127.0.0.1", NAME_SERVICE_PORT);
            }
            Console.WriteLine("running... Press space to see status.");
            while (true)
            {
                PrintConnections();
                unifiedNetwork.Update();
                Thread.Sleep(0);
            }

            factory.Reset();
            unifiedNetwork.Dispose();
#endif
        }

        private static void SimpleNetworkMain(string[] args)
        {
            
        }

        private static void PrintConnections()
        {
#if false
            if (_kbhit() != 0)
            {
                char key = Console.ReadKey(true).KeyChar;
                if (key == ' ')
                {
                    Console.Write("--------------------------------\n");
                    uint numPeers = UnifiedNetwork.Instance.ServerInterface.NumberOfConnections();
                    //uint numPeers = 10;

                    Console.Write("{0} (Conn): ", serverPort);
                    for (int j = 0; j < numPeers; j++)
                    {
                        SystemAddress systemAddress =
                            UnifiedNetwork.Instance.ServerInterface.GetSystemAddressFromIndex(j);
                        if (!systemAddress.Equals(RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS))
                            Console.Write("{0} ", systemAddress.port);
                    }

                    Console.Write("\n");
                    Console.Write("\n");

                    Console.Write("--------------------------------\n");
                }
                else if (key == 't')
                {
                    IComplecatedEvent _event = new TestConnectionEvent2((int) SampleEventFactory.EventTypes.TESTCONNECTION2);

                    ServiceConfigurator.Resolve<SampleEventFactory>().StoreExternallyCreatedEvent(_event);
                    UnifiedNetwork.Instance.SendEvent(_event, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS);
                }
                key = '\0';
            }
#endif
        }

        [DllImport("crtdll.dll")]
        public static extern int _kbhit(); // I do not want to use this.
        private static ushort serverPort;

        #endregion
    }
}