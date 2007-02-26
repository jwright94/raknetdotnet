using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using Castle.Core;
    using Castle.MicroKernel;
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

        static void UnifiedNetworkMain(string[] args)
        {
            Dictionary<string, object> extendedProperties = new Dictionary<string, object>();
            extendedProperties.Add("port", (ushort)6000);
            UnifiedNetwork unifiedNetwork = new UnifiedNetwork("server.xml", extendedProperties);
            RpcCalls rpcCalls = new RpcCalls();
            rpcCalls.ProcessEventOnServerSide += unifiedNetwork.ProcessEvent;
            SampleEventFactory factory = new SampleEventFactory();
            rpcCalls.Handler = factory;

            unifiedNetwork.Start();

            factory.Dispose();
            unifiedNetwork.Dispose();
        }
    }
}
