using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectMemberRPC
{
    using RakNetDotNet;

    class Apple
    {
        public Apple()
        {
            gen.Parent = this;
        }

        public virtual void func1(RPCParameters rpcParams)
        {
            byte[] input = rpcParams.input;
            if (0 < input.Length)
                Console.Write("Base Apple func1: {0}\n", GetString(input));
            else
                Console.Write("Base Apple func1\n");
        }

        public virtual void func1(string blah)
        {
            Console.Write("Func1.  Does not match function signature and should never be called.\n");
        }

        public virtual void func2(RPCParameters rpcParams)
        {
            byte[] input = rpcParams.input;
            if(0 < input.Length)
                Console.Write("Base Apple func2: {0}\n", GetString(input));
            else
                Console.Write("Base Apple func2\n");
        }

        public virtual void func3(RPCParameters rpcParams)
        {
            byte[] input = rpcParams.input;
            if(0 < input.Length)
                Console.Write("Base Apple func3: {0}\n", GetString(input));
            else
                Console.Write("Base Apple func3\n");
        }

        protected string GetString(byte[] input)
        {
            Encoding encoder = new UnicodeEncoding();
            return encoder.GetString(input);
        }

        private ServerNetworkIDGenerator gen = new ServerNetworkIDGenerator();
    }

    class GrannySmith : Apple
    {
        public override void func1(RPCParameters rpcParams)
        {
            Console.Write("Derived GrannySmith func1: {0}\n", GetString(rpcParams.input));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            uint time;
            RakPeerInterface rakPeer1 = RakNetworkFactory.GetRakPeerInterface();
            RakPeerInterface rakPeer2 = RakNetworkFactory.GetRakPeerInterface();

            Console.Write("This project shows how to call member functions of your application object.\n");
            Console.Write("instances.\n");
            Console.Write("Difficulty: Advanced\n\n");

            SocketDescriptor socketDescriptor = new SocketDescriptor(60000, string.Empty);
            rakPeer1.Startup(2, 0, new SocketDescriptor[] { socketDescriptor }, 1);
            socketDescriptor.port = 60001;
            rakPeer2.Startup(2, 0, new SocketDescriptor[] { socketDescriptor }, 1);
            rakPeer2.SetMaximumIncomingConnections(2);
            rakPeer1.Connect("127.0.0.1", 60001, string.Empty, 0);

            rakPeer2.RegisterClassMemberRPC(typeof(Apple).GetMethod("func1"));
            rakPeer2.RegisterClassMemberRPC(typeof(Apple).GetMethod("func2"));
            rakPeer2.RegisterClassMemberRPC(typeof(Apple).GetMethod("func3"));

            System.Threading.Thread.Sleep(250);

            Apple apple = new GrannySmith();
            time = RakNetBindings.GetTime();

            //Console.Write("Calling func1 of Apple base class with test string 1.\n");
            //rakPeer1.RPC(typeof(Apple).GetMethod("func1"), "test string 1", (int)(strlen("test string 1") + 1) * 8, HIGH_PRIORITY, RELIABLE_ORDERED, 0, UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple->GetNetworkID(), 0);
            //Console.Write("Calling func2 of Apple base class with test string 2.\n");
            //rakPeer1.RPC(CLASS_MEMBER_ID(Apple, func2), "test string 2", (int)(strlen("test string 2") + 1) * 8, HIGH_PRIORITY, RELIABLE_ORDERED, 0, UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple->GetNetworkID(), 0);
            //Console.Write("Calling func3 of Apple base class with no test string.\n");
            //rakPeer1.RPC(CLASS_MEMBER_ID(Apple, func3), 0, 0, HIGH_PRIORITY, RELIABLE_ORDERED, 0, UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple->GetNetworkID(), 0);

        }
    }
}
