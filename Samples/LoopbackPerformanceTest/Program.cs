using System;
using System.Collections.Generic;
using System.Text;

namespace LoopbackPerformanceTest
{
    using RakNetDotNet;

    class Program
    {
        const ushort DESTINATION_SYSTEM_PORT = 60000;
        const ushort RELAY_SYSTEM_PORT = 60001;
        const ushort SOURCE_SYSTEM_PORT = 60002;

        static int Main(string[] args)
        {
            RakPeerInterface localSystem;
            Packet p;
            int systemType, threadSleepTimer;
            byte[] byteBlock = new byte[4096];
            string input;
            StringBuilder buffer = new StringBuilder(4096);
            uint time, quitTime, nextStatsTime;
            uint packetsPerSecond = 0, bytesPerPacket = 0, num, index, bytesInPackets;
            uint lastSendTime;
            int sendMode = 0;
            int verbosityLevel;
            uint showStatsInterval;
            bool connectionCompleted, incomingConnectionCompleted;
            RakNetStatistics rss;

            Console.Write("Loopback performance test.\n");
            Console.Write("This test measures the effective transfer rate of RakNetBindings.\n\n");
            Console.Write("Instructions:\nStart 3 instances of this program.\n");
            Console.Write("Press\n1. for the first instance (destination)\n2. for the second instance (relay)\n3. for the third instance (source).\n");
            Console.Write("When the third instance is started the test will start.\n\n");
            Console.Write("Difficulty: Intermediate\n\n");
            Console.Write("Which instance is this?  Enter 1, 2, or 3: ");

            input = Console.ReadLine();
            systemType = input[0] - '0' - 1;
            if (systemType < 0 || systemType > 2)
            {
                Console.Write("Error, you must enter 1, 2, or 3.\nQuitting.\n");
                return 1;
            }

            localSystem = RakNetworkFactory.GetRakPeerInterface();
            /*
            Console.Write("Enter thread sleep timer:\n(0). Regular\n(1). High\n");
            byteBlock = Console.ReadLine();
            if (byteBlock[0]==0)
            {
                Console.Write("Defaulting to regular.\n");
                threadSleepTimer=0;
            }
            else
            {
                if (byteBlock[0]<'0' || byteBlock[0]>'1')
                {
                    Console.Write("Error, you must enter 0, or 1\n.Quitting.\n");
                    return 1;
                }
                threadSleepTimer=byteBlock[0]-'0';
            }
            */
            threadSleepTimer = 0;

            Console.Write("How many seconds do you want to run the test for?\n");

            input = Console.ReadLine();
            if (input.Equals(string.Empty))
            {
                Console.Write("Defaulting to 90 seconds\n");
                quitTime = 90;
            }
            else
                quitTime = uint.Parse(input);

            Console.Write("Enter statistics verbosity level, 0=lowest, 2=highest\n");
            input = Console.ReadLine();
            if (input.Equals(string.Empty))
            {
                Console.Write("Defaulting to verbosity level 1\n");
                verbosityLevel = 1;
            }
            else
                verbosityLevel = int.Parse(input);

            Console.Write("How frequently to show statistics, in seconds?\n");
            input = Console.ReadLine();
            if (input.Equals(string.Empty))
            {
                Console.Write("Defaulting to 5 seconds\n");
                showStatsInterval = 5 * 1000;
            }
            else
                showStatsInterval = uint.Parse(input) * 1000;

            if (systemType == 0)
            {
                Console.Write("Initializing RakNetBindings...\n");
                // Destination.  Accept one connection and wait for further instructions.
                SocketDescriptor socketDescriptor = new SocketDescriptor(DESTINATION_SYSTEM_PORT, string.Empty);
                if (localSystem.Startup(1, threadSleepTimer, new SocketDescriptor[] { socketDescriptor }, 1) == false)
                {
                    Console.Write("Failed to initialize RakNet!.\nQuitting\n");
                    return 1;
                }
                localSystem.SetMaximumIncomingConnections(1);
                Console.Write("Initialization complete. Destination system started and waiting...\n");
            }
            else if (systemType == 1)
            {
                Console.Write("Enter MTU size to use.  576 for dialup, 1400 for AOL, 1492 otherwise.\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
                    Console.Write("Defaulting to 1492.\n");
                    localSystem.SetMTUSize(1492, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS);
                }
                else
                    localSystem.SetMTUSize(int.Parse(input), RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS);

                Console.Write("What send mode to use for relays?\n");
                Console.Write("(0). UNRELIABLE\n");
                Console.Write("(1). UNRELIABLE_SEQUENCED\n");
                Console.Write("(2). RELIABLE\n");
                Console.Write("(3). RELIABLE_ORDERED\n");
                Console.Write("(4). RELIABLE_SEQUENCED\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
                    Console.Write("Defaulting to RELIABLE\n");
                    sendMode = 2;
                }
                else
                {
                    sendMode = int.Parse(input);
                    if (sendMode < 0 || sendMode > 4)
                    {
                        Console.Write("Invalid send mode.  Using UNRELIABLE\n");
                        sendMode = 0;
                    }
                }

                Console.Write("Initializing RakNetBindings...\n");
                // Relay.  Accept one connection, initiate outgoing connection, wait for further instructions.
                SocketDescriptor socketDescriptor = new SocketDescriptor(RELAY_SYSTEM_PORT, string.Empty);
                if (localSystem.Startup(2, threadSleepTimer, new SocketDescriptor[] { socketDescriptor }, 1) == false)
                {
                    Console.Write("Failed to initialize RakNet!.\nQuitting\n");
                    return 1;
                }
                localSystem.SetMaximumIncomingConnections(1);
                socketDescriptor.port = DESTINATION_SYSTEM_PORT;
                if (localSystem.Connect("127.0.0.1", DESTINATION_SYSTEM_PORT, string.Empty, 0) == false)
                {
                    Console.Write("Connect call failed!.\nQuitting\n");
                    return 1;
                }

                Console.Write("Initialization complete. Relay system started.\nConnecting to destination and waiting for sender...\n");
            }
            else
            {
                Console.Write("Enter MTU size to use. 576 for dialup, 1400 for AOL, 1492 otherwise.\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
                    Console.Write("Defaulting to 1492.\n");
                    localSystem.SetMTUSize(1492, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS);
                }
                else
                    localSystem.SetMTUSize(int.Parse(input), RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS);

                Console.Write("How many packets do you wish to send per second?\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
#if DEBUG
                    Console.Write("Defaulting to 1000\n");
                    packetsPerSecond = 1000;
#else
                    Console.Write("Defaulting to 10000\n");
                    packetsPerSecond=10000;
#endif
                }
                else
                    packetsPerSecond = uint.Parse(input);
                Console.Write("How many bytes per packet?\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
                    Console.Write("Defaulting to 400\n");
                    bytesPerPacket = 400;
                }
                else
                {
                    bytesPerPacket = uint.Parse(input);
                    if (bytesPerPacket > 4096)
                    {
                        Console.Write("Increase the array size of byteBlock to send more than 4096 bytes.\n");
                        bytesPerPacket = 4096;
                    }
                }

                Console.Write("What send mode?\n");
                Console.Write("(0). UNRELIABLE\n");
                Console.Write("(1). UNRELIABLE_SEQUENCED\n");
                Console.Write("(2). RELIABLE\n");
                Console.Write("(3). RELIABLE_ORDERED\n");
                Console.Write("(4). RELIABLE_SEQUENCED\n");
                input = Console.ReadLine();
                if (input.Equals(string.Empty))
                {
                    Console.Write("Defaulting to RELIABLE\n");
                    sendMode = 2;
                }
                else
                {
                    sendMode = int.Parse(input);
                    if (sendMode < 0 || sendMode > 4)
                    {
                        Console.Write("Invalid send mode.  Using UNRELIABLE\n");
                        sendMode = 0;
                    }
                }

                Console.Write("Initializing RakNetBindings...\n");
                // Sender.  Initiate outgoing connection to relay.
                SocketDescriptor socketDescriptor = new SocketDescriptor(SOURCE_SYSTEM_PORT, string.Empty);
                if (localSystem.Startup(1, threadSleepTimer, new SocketDescriptor[] { socketDescriptor }, 1) == false)
                {
                    Console.Write("Failed to initialize RakNet!.\nQuitting\n");
                    return 1;
                }
                if (localSystem.Connect("127.0.0.1", RELAY_SYSTEM_PORT, string.Empty, 0) == false)
                {
                    Console.Write("Connect call failed!.\nQuitting\n");
                    return 1;
                }

                Console.Write("Initialization complete. Sender system started. Connecting to relay...\n");
            }

            connectionCompleted = false;
            incomingConnectionCompleted = false;
            time = RakNetBindings.GetTime();
            lastSendTime = time;
            nextStatsTime = time + 2000; // First stat shows up in 2 seconds
            bytesInPackets = 0;

            while (time < quitTime || (connectionCompleted == false && incomingConnectionCompleted == false))
            {
                time = RakNetBindings.GetTime();
                // Parse messages
                while (true)
                {
                    p = localSystem.Receive();

                    if (p != null)
                    {
                        bytesInPackets += p.length;
                        BitStream inBitStream = new BitStream(p, false);
                        byte packetIdentifier;
                        inBitStream.Read(out packetIdentifier);
                        inBitStream.ResetReadPointer();
                        switch (packetIdentifier)
                        {
                            case RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED:
                                Console.Write("ID_CONNECTION_REQUEST_ACCEPTED.\n");
                                connectionCompleted = true;
                                // Timer starts when a connection has completed
                                if (systemType == 1 || systemType == 2)
                                    quitTime = quitTime * 1000 + time;
                                break;
                            case RakNetBindings.ID_DISCONNECTION_NOTIFICATION:
                                // Connection lost normally
                                Console.Write("ID_DISCONNECTION_NOTIFICATION.\n");
                                //		connectionCompleted=false;
                                break;
                            case RakNetBindings.ID_NEW_INCOMING_CONNECTION:
                                // Somebody connected.  We have their IP now
                                Console.Write("ID_NEW_INCOMING_CONNECTION.\n");
                                incomingConnectionCompleted = true;
                                // Timer starts when a new connection has come in
                                if (systemType == 0)
                                    quitTime = quitTime * 1000 + time;
                                if (systemType == 1 && connectionCompleted == false)
                                    Console.Write("Warning, relay connection to destination has not completed yet.\n");
                                break;

                            case RakNetBindings.ID_CONNECTION_LOST:
                                // Couldn't deliver a reliable packet - i.e. the other system was abnormally
                                // terminated
                                Console.Write("ID_CONNECTION_LOST.\n");
                                //	connectionCompleted=false;
                                break;
                            case RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS:
                                Console.Write("ID_NO_FREE_INCOMING_CONNECTIONS.\n");
                                break;
                            default:
                                // The relay system will relay all data with 255 as the first byte
                                if (systemType == 1)
                                {
                                    if (packetIdentifier == 255)
                                    {
                                        if (localSystem.Send(inBitStream, PacketPriority.HIGH_PRIORITY, (PacketReliability)sendMode, 0, p.systemAddress, true) == false)
                                        {
                                            Console.Write("Relay failed!\n");
                                        }
                                    }
                                    else
                                        Console.Write("Got packet with ID {0}\n", packetIdentifier);
                                }

                                break;
                        }
                    }
                    else
                        break;

                    localSystem.DeallocatePacket(p);
                }

                // Show stats.
                if (time > nextStatsTime && (connectionCompleted || incomingConnectionCompleted))
                {
                    Console.Write("\n* First connected system statistics:\n");
                    rss = localSystem.GetStatistics(localSystem.GetSystemAddressFromIndex(0));
                    RakNetBindings.StatisticsToString(rss, buffer, verbosityLevel);
                    Console.Write("{0}", buffer.ToString());
                    if (systemType == 1)
                    {
                        rss = localSystem.GetStatistics(localSystem.GetSystemAddressFromIndex(1));
                        if (rss != null)
                        {
                            Console.Write("* Second connected system statistics:\n");
                            RakNetBindings.StatisticsToString(rss, buffer, verbosityLevel);
                            Console.Write("{0}", buffer.ToString());
                        }
                    }

                    nextStatsTime = time + showStatsInterval;
                }

                // As the destination, we don't care if the connection is completed.  Do nothing
                // As the relay, we relay packets if the connection is completed.
                // That is done when the packet arrives.
                // As the source, we start sending packets when the connection is completed.
                if (systemType == 2 && connectionCompleted)
                {

                    // Number of packets to send is (float)(packetsPerSecond * (time - lastSendTime)) / 1000.0f;
                    num = (packetsPerSecond * (uint)(time - lastSendTime)) / 1000;
                    byteBlock[0] = 255; // Relay all data with an identifier of 255
                    for (index = 0; index < num; index++)
                    {
                        localSystem.Send(byteBlock, (int)bytesPerPacket, PacketPriority.HIGH_PRIORITY, (PacketReliability)sendMode, 0, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                    }

                    lastSendTime += (1000 * num) / packetsPerSecond;
                }

                System.Threading.Thread.Sleep(100);
            }

            Console.Write("Test duration elapsed.  Final Stats:\n");
            Console.Write("\n* First connected system statistics:\n");
            rss = localSystem.GetStatistics(localSystem.GetSystemAddressFromIndex(0));
            RakNetBindings.StatisticsToString(rss, buffer, 2);
            Console.Write("{0}", buffer.ToString());
            if (systemType == 1)
            {
                rss = localSystem.GetStatistics(localSystem.GetSystemAddressFromIndex(1));
                if (rss != null)
                {
                    Console.Write("* Second connected system statistics:\n");
                    RakNetBindings.StatisticsToString(rss, buffer, 2);
                    Console.Write("{0}", buffer.ToString());
                }
            }

            Console.Write("Hit enter to continue.\n");

            string buff;
            buff = Console.ReadLine();

            RakNetworkFactory.DestroyRakPeerInterface(localSystem);
            return 0;

        }
    }
}
