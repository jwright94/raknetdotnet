using System;
using System.Collections.Generic;
using System.Text;

namespace BitStream
{
    using RakNetDotNet;

    class Program
    {
        static bool quit;

        struct EmploymentStruct
        {
            public int salary;
            public byte yearsEmployed;
        }

        public static void clientRPC(RPCParameters rpcParameters)
        {
            BitStream b = new BitStream(rpcParameters, false);
            string name;

            if (!b.Read(out name))
            {
                Console.Write("Too-short bitstreams.\n");
                return;
            }

            Console.Write("In clientRPC:\n");
            Console.Write("Name is {0}\n", name);

            uint age;
            if (!b.ReadCompressed(out age))
            {
                return;
            }

            Console.Write("Age is {0}\n", age);
            Console.Out.Flush();

            bool wroteEmploymentStruct;
            if (!b.Read(out wroteEmploymentStruct))
            {
                return;
            }

            if (wroteEmploymentStruct)
            {
                Console.Write("We are employed.\n");

                EmploymentStruct employmentStruct;
                if (!b.Read(out employmentStruct.salary)) return;
                if (!b.Read(out employmentStruct.yearsEmployed)) return;

                Console.Write("Salary is {0}.  Years employed is {1}\n", employmentStruct.salary, employmentStruct.yearsEmployed);
            }
            else
                Console.Write("We are between jobs :)\n");

            quit = true;
        }

        static void Main(string[] args)
        {
            RakPeerInterface rakClient = RakNetworkFactory.GetRakPeerInterface();
            RakPeerInterface rakServer = RakNetworkFactory.GetRakPeerInterface();

            quit = false;
            string text;

            rakClient.RegisterAsRemoteProcedureCall(typeof(Program).GetMethod("clientRPC"));

            SocketDescriptor socketDescriptor = new SocketDescriptor(2000, string.Empty);
            if (!rakServer.Startup(1, 30, new SocketDescriptor[] { socketDescriptor }, 1))
            {
                Console.Write("Start call failed!\n");
                return;
            }
            rakServer.SetMaximumIncomingConnections(1);
            socketDescriptor.port = 2100;
            rakClient.Startup(1, 30, new SocketDescriptor[] { socketDescriptor }, 1);
            if (!rakClient.Connect("127.0.0.1", 2000, string.Empty, 0))
            {
                Console.Write("Connect call failed\n");
                return;
            }

            BitStream outgoingBitstream = new BitStream();
            uint age;

            Console.Write("A sample on how to use RakNet's bitstream class\n");
            Console.Write("Difficulty: Beginner\n\n");

            Console.Write("Enter your name.\n");
            text = Console.ReadLine();
            if (text.Equals(string.Empty))
                text = "Unnamed!";
            outgoingBitstream.Write(text);

            Console.Write("Enter your age (numbers only).\n");
            text = Console.ReadLine();
            if (text.Equals(string.Empty))
                age = 0;
            else
                age = uint.Parse(text);

            outgoingBitstream.WriteCompressed(age);

            Console.Write("Are you employed (y/n)?\n");
            text = Console.ReadLine();
            if (text == "y")
            {
                outgoingBitstream.Write(true);

                // Read some data into a struct
                EmploymentStruct employmentStruct;
                Console.Write("What is your salary (enter a number only)?\n");
                text = Console.ReadLine();
                employmentStruct.salary = int.Parse(text);
                Console.Write("How many years have you been employed (enter a number only)?\n");
                text = Console.ReadLine();
                employmentStruct.yearsEmployed = byte.Parse(text);

                // We can write structs to a bitstream but this is not portable due to:
                //  1. Different-endian CPUs
                //  2. Different 'padding' of structs depending on compiler, etc
                // The only safe way to send a struct is by using the BitStream
                // to write out every single member which you want to send.
                outgoingBitstream.Write(employmentStruct.salary);
                outgoingBitstream.Write(employmentStruct.yearsEmployed);
                // We're done writing to the struct
            }
            else
            {
                //Console.Write("Number of bits before [false]: %d\n",
                //outgoingBitstream.GetNumberOfBitsUsed() );
                outgoingBitstream.Write(false); // Writing a bool takes 1 bit
                // We're done writing to the struct.  Compare this to the example above - we wrote quite a bit less.
            }

            bool success = rakServer.RPC(typeof(Program).GetMethod("clientRPC"), outgoingBitstream, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, 0, RakNetBindings.UNASSIGNED_NETWORK_ID, null);
            if (!success)
                Console.Write("RPC call failed\n");

            while (!quit)
            {
                rakClient.DeallocatePacket(rakClient.Receive());
                rakServer.DeallocatePacket(rakServer.Receive());

                System.Threading.Thread.Sleep(30);
            }

            Console.Write("Press enter to quit\n");
            Console.ReadLine();

            rakClient.Shutdown(100, 0);
            rakServer.Shutdown(100, 0);

            // This is not necessary since on shutdown everything is unregistered.  This is just here to show usage
            rakClient.UnregisterAsRemoteProcedureCall(typeof(Program).GetMethod("clientRPC"));

            RakNetworkFactory.DestroyRakPeerInterface(rakClient);
            RakNetworkFactory.DestroyRakPeerInterface(rakServer);
        }
    }
}
