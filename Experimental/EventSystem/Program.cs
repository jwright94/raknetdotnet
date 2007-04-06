using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;
using CommandLine;
using SampleEvents;

namespace EventSystem
{
    internal class AppArguments
    {
        [DefaultArgumentAttribute(ArgumentType.Required, HelpText = "Configuration xml filename.")]
        public string ConfigurationFilename;
    }

    interface IServer : IDisposable
    {
        void Startup();
        void Update();
    }

    [Transient]
    internal sealed class NamingServer : IServer
    {
        private readonly ILogger logger;
        private ICommunicator communicator;

        public NamingServer(ILogger logger)
        {
            this.logger = logger;
        }
        public void Startup()
        {
            communicator = LightweightContainer.Resolve<ICommunicator>();
            EventHandlersOnNamingServer handlers = new EventHandlersOnNamingServer();
            handlers.Register += Handlers_OnRegister;
            communicator.ProcessorsLocator = new NamingServerPPLocator(handlers);   // inject manually
            communicator.Startup();
        }

        private void Handlers_OnRegister(SampleEvents.RegisterEvent t)
        {
            logger.Debug("Handlers_OnRegister");
        }

        public void Update()
        {
            communicator.Update();
        }

        public void Dispose()
        {
            LightweightContainer.ReleaseComponent(communicator);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            AppArguments parsedArgs = new AppArguments();
            if(!Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                return;
            }

            LightweightContainer.Configure(parsedArgs.ConfigurationFilename);
            ILogger logger = LightweightContainer.LogFactory.Create("Global");
            IServer server = LightweightContainer.Resolve<IServer>();
            server.Startup();
            logger.Info("Server is started.");
            while(true)
            {
                if(_kbhit() != 0) {
                    break;
                }
                server.Update();
            }
            LightweightContainer.ReleaseComponent(server);
            logger.Info("Server is shutdowned.");
        }

        [Obsolete]
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

        [Obsolete]
        private static void ServerMain(string[] args)
        {
            EventCenterServer server = new EventCenterServer("server.xml");
            SampleEventFactory factory = LightweightContainer.Resolve<SampleEventFactory>();
            factory.Reset();
            RpcCalls rpcCalls = LightweightContainer.Resolve<RpcCalls>();
            rpcCalls.Reset();
            rpcCalls.ProcessEventOnServerSide += server.ProcessEvent;

            server.Start();

            factory.Reset();
            server.Dispose();
        }

        #region Unified Network

        [Obsolete]
        private static void UnifiedNetworkMain(string[] args)
        {
            UnifiedNetwork network = LightweightContainer.Resolve<UnifiedNetwork>();
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

        [Obsolete]
        private static void SimpleNetworkMain(string[] args)
        {
            
        }

        [Obsolete]
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