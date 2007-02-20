// TODO - rewrite.
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
            netObj.Parent = this;
        }

        public virtual void func1(RPCParameters rpcParams)
        {
            if (0 < rpcParams.numberOfBitsOfData)
                Console.Write("Base Apple func1: {0}\n", GetString(rpcParams.input));
            else
                Console.Write("Base Apple func1\n");
        }

        // RakNetDotNet does not support this now.
        //public virtual void func1(string blah)
        //{
        //    Console.Write("Func1.  Does not match function signature and should never be called.\n");
        //}

        public virtual void func2(RPCParameters rpcParams)
        {
            if(0 < rpcParams.numberOfBitsOfData)
                Console.Write("Base Apple func2: {0}\n", GetString(rpcParams.input));
            else
                Console.Write("Base Apple func2\n");
        }

        public virtual void func3(RPCParameters rpcParams)
        {
            if(0 < rpcParams.numberOfBitsOfData)
                Console.Write("Base Apple func3: {0}\n", GetString(rpcParams.input));
            else
                Console.Write("Base Apple func3\n");
        }

        public virtual NetworkID GetNetworkID()
        {
            return netObj.GetNetworkID();
        }

        protected string GetString(byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes);
        }

        protected NetworkIDObject netObj = new NetworkIDObject();  // I recommend that your class does not inherit from NetworkIDObject.
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

            System.Threading.Thread.Sleep(250 * 4);  // Please wait slightly longer.

            Apple apple = new GrannySmith();
            time = RakNetBindings.GetTime();

            Console.Write("Calling func1 of Apple base class with test string 1.\n");
            // You should use interface. Clients should not know server implements.
            rakPeer1.RPC(typeof(Apple).GetMethod("func1"), GetBytes("test string 1"), (uint)(GetBytes("test string 1").Length * 8), PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple.GetNetworkID(), null);
            // Or you can pass full method name.
            // rakPeer1.RPC("ObjectMemberRPC.Apple.func1", GetBytes("test string 1"), (uint)(GetBytes("test string 1").Length * 8), PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple.GetNetworkID(), null);
            Console.Write("Calling func2 of Apple base class with test string 2.\n");
            rakPeer1.RPC(typeof(Apple).GetMethod("func2"), GetBytes("test string 2"), (uint)(GetBytes("test string 2").Length * 8), PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple.GetNetworkID(), null);
            Console.Write("Calling func3 of Apple base class with no test string.\n");
            rakPeer1.RPC(typeof(Apple).GetMethod("func3"), new byte[0], 0, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, 0, apple.GetNetworkID(), null);

            while (RakNetBindings.GetTime() < time + 5000)
            {
                rakPeer1.DeallocatePacket(rakPeer1.Receive());
                rakPeer2.DeallocatePacket(rakPeer2.Receive());
                System.Threading.Thread.Sleep(30);
            }

            Console.Write("Sample complete.  Press enter to quit.");
            Console.ReadLine();

            RakNetworkFactory.DestroyRakPeerInterface(rakPeer1);
            RakNetworkFactory.DestroyRakPeerInterface(rakPeer2);
        }

        static byte[] GetBytes(string s)
        {
            return Encoding.Unicode.GetBytes(s);
        }
    }
}
