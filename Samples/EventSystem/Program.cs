using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using Castle.Core;
    using Castle.MicroKernel;
    using RakNetDotNet;
    #region Castle MicroKernel
    //[Transient]
    class Automobile : IDisposable
    {
        public Automobile(IKernel kernel, Tire _tire, string _name)
        {
            tire = _tire;
        }
        public void Drive()
        {
            Console.WriteLine(name);
            tire.Roll();
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string name;
        Tire tire;

        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }

        #region IStartable ÉÅÉìÉo

        public void Start()
        {
            Console.WriteLine("Start");
        }

        public void Stop()
        {
            Console.WriteLine("Stop");
        }

        #endregion
    }

    class Tire
    {
        public void Roll()
        {
            Console.WriteLine("Rolling Tire.");
        }
    }

    class Main
    {
        public void Test()
        {
            IKernel k = ConfigureContainer();
            //Automobile a = (Automobile)k[typeof(Automobile)];
            Dictionary<string, object> arg = new Dictionary<string,object>();
            arg["_name"] = "Mama";
            Automobile a = (Automobile)k.Resolve("automobile", arg);
            a.Name = "GT";
            a.Drive();
            k.ReleaseComponent(a);
        }

        IKernel ConfigureContainer()
        {
            IKernel kernel = new DefaultKernel();
            kernel.AddComponent("tire", typeof(Tire));
            kernel.AddComponent("automobile", typeof(Automobile));
            return kernel;
        }
    }
    #endregion
    class Program
    {
        static void Main(string[] args)
        {
            //Main m = new Main();
            //m.Test();
            //char keyp = Console.ReadKey(true).KeyChar;
            //return;

            Console.WriteLine("(S)erver or (U)nifiedNetwork or (C)lient?");
            char key = Console.ReadKey(true).KeyChar;
            if (key == 's' || key == 'S')
                ServerMain(args);
            else if (key == 'u' || key == 'U')
                UnifiedNetworkMain(args);
            else
                ClientMain(args);
        }
        static void ClientMain(string[] args)
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
        static void ServerMain(string[] args)
        {
            EventCenterServer server = new EventCenterServer("server.xml");
            RpcCalls rpcCalls = new RpcCalls();
            rpcCalls.ProcessEventOnServerSide += server.ProcessEvent;
            SampleEventFactory factory = new SampleEventFactory();
            rpcCalls.Handler = factory;

            server.Start();

            factory.Dispose();
            server.Dispose();
        }
        #region Unified Network
        static void UnifiedNetworkMain(string[] args)
        {
            const ushort NAME_SERVICE_PORT = 6000;

            Console.WriteLine("Server Port ? (NS=6000)");
            string input = Console.ReadLine();
            if (input.Equals(string.Empty))
                serverPort = NAME_SERVICE_PORT;
            else
                serverPort = ushort.Parse(input);

            Dictionary<string, object> extendedProperties = new Dictionary<string, object>();
            extendedProperties.Add("allowedPlayers", (ushort)10);
            extendedProperties.Add("port", serverPort);
            UnifiedNetwork unifiedNetwork = new UnifiedNetwork("server.xml", extendedProperties);
            RpcCalls rpcCalls = new RpcCalls();
            rpcCalls.ProcessEventOnServerSide += unifiedNetwork.ProcessEvent;
            SampleEventFactory factory = new SampleEventFactory();
            rpcCalls.Handler = factory;

            if (serverPort != NAME_SERVICE_PORT)
            {
                unifiedNetwork.ConnectPlayer("127.0.0.1", NAME_SERVICE_PORT);
            }
            System.Console.WriteLine("running...");
            while (true)
            {
                PrintConnections();
                unifiedNetwork.Update();
                System.Threading.Thread.Sleep(0);
            }

            factory.Dispose();
            unifiedNetwork.Dispose();
        }
        static void PrintConnections()
        {
            if (_kbhit() != 0)
            {
                char key = Console.ReadKey(true).KeyChar;
                if (key == ' ')
                {
                    Console.Write("--------------------------------\n");
                    //uint numPeers = UnifiedNetwork.Instance.ServerInterface.GetNumberOfAddresses();  // TODO - Maybe return wrong number.
                    uint numPeers = 10;

                    Console.Write("{0} (Conn): ", serverPort);
                    for (int j = 0; j < numPeers; j++)
                    {
                        SystemAddress systemAddress = UnifiedNetwork.Instance.ServerInterface.GetSystemAddressFromIndex(j);
                        if (!systemAddress.Equals(RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS))
                            Console.Write("{0} ", systemAddress.port);
                    }

                    Console.Write("\n");
                    Console.Write("\n");
                    key = '\0';

                    Console.Write("--------------------------------\n");
                }
            }
        }
        [System.Runtime.InteropServices.DllImport("crtdll.dll")]
        public static extern int _kbhit();  // I do not want to use this.
        static ushort serverPort;
        #endregion
    }
}
