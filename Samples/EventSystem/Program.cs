using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("(S)erver or (C)lient?");
            char key = Console.ReadKey(true).KeyChar;
            if (key == 's' || key == 'S')
                ServerMain(args);
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
            SampleEventFactory factory = new SampleEventFactory();
            rpcCalls.Handler = factory;

            server.Start();

            factory.Dispose();
            server.Dispose();
        }
    }
}
