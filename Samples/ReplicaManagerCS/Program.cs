using System;
using System.Collections.Generic;
using System.Text;

namespace ReplicaManagerCS
{
    using RakNetDotNet;

    class Program
    {
        public static bool isServer;
        static RakPeerInterface rakPeer;

        public static ReplicaManagerExt replicaManager = new ReplicaManagerExt();

        public static Monster monster;
        public static Player player;

        static ReplicaReturnResult ConstructionCB(BitStream inBitStream, uint timestamp, NetworkID networkID, Replica existingReplica, SystemAddress senderId, ReplicaManagerExt caller, IntPtr userData)
        {
            StringBuilder output = new StringBuilder(255);

            if (isServer)
                return ReplicaReturnResult.REPLICA_PROCESSING_DONE;

            StringTable.Instance().DecodeString(output, output.Capacity, inBitStream);
            if (output.ToString() == "Player")
            {
                System.Diagnostics.Debug.Assert(player == null);

                player = new Player();

                player.replica.SetNetworkID(networkID);

                if (!isServer)
                {
                    replicaManager.Construct(player.replica, true, senderId, false);

                    replicaManager.SetScope(player.replica, true, senderId, false);
                }

                Console.Write("New player created\n");
            }
            else if (output.ToString() == "Monster")
            {
                System.Diagnostics.Debug.Assert(monster == null);

                monster = new Monster();

                monster.replica.SetNetworkID(networkID);

                if (!isServer)
                {
                    replicaManager.Construct(monster.replica, true, senderId, false);

                    replicaManager.SetScope(monster.replica, true, senderId, false);
                }

                Console.Write("New monster created\n");
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        static ReplicaReturnResult SendDownloadCompleteCB(BitStream inBitStream, uint timestamp, SystemAddress senderId, ReplicaManagerExt caller, IntPtr userData)
        {
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        static ReplicaReturnResult ReceiveDownloadCompleteCB(BitStream inBitStream, SystemAddress senderId, ReplicaManagerExt caller, IntPtr userData)
        {
            if (!isServer)
                Console.Write("Object downloads complete\n");
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        static int Main(string[] args)
        {
            char ch;
            string userInput;

            rakPeer = RakNetworkFactory.GetRakPeerInterface();

            rakPeer.AttachPlugin(replicaManager);

            replicaManager.SetAutoParticipateNewConnections(true);

            replicaManager.SetAutoConstructToNewParticipants(true);

            replicaManager.SetAutoSerializeInScope(true);

            replicaManager.SetReceiveConstructionCB(IntPtr.Zero, ConstructionCB);

            replicaManager.SetDownloadCompleteCB(IntPtr.Zero, SendDownloadCompleteCB, IntPtr.Zero, ReceiveDownloadCompleteCB);

            StringTable.Instance().AddString("Player", true);
            StringTable.Instance().AddString("Monster", true);

            Console.Write("Demonstration of ReplicaManager for client / server\n");
            Console.Write("The replica manager provides a framework to make it easier to synchronize\n");
            Console.Write("object creation, destruction, and member object updates\n");
            Console.Write("Difficulty: Intermediate\n\n");
            Console.Write("Run as (s)erver or (c)lient? ");
            userInput = Console.ReadLine();
            if (userInput[0] == 's' || userInput[0] == 'S')
            {
                isServer = true;
                SocketDescriptor socketDescriptor = new SocketDescriptor(60000, string.Empty);
                rakPeer.Startup(8, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                rakPeer.SetMaximumIncomingConnections(8);
                Console.Write("Server started.\n");
            }
            else
            {
                isServer = false;
                SocketDescriptor socketDescriptor = new SocketDescriptor();
                rakPeer.Startup(1, 0, new SocketDescriptor[] { socketDescriptor }, 1);
                Console.Write("Enter IP to connect to: ");
                userInput = Console.ReadLine();
                if (userInput.Equals(string.Empty))
                {
                    userInput = "127.0.0.1";
                    Console.Write("{0}\n", userInput);
                }
                if (!rakPeer.Connect(userInput, 60000, string.Empty, 0))
                {
                    Console.Write("Connect call failed!\n");
                    return 1;
                }
                Console.Write("Connecting...\n");
            }

            Console.Write("Commands:\n(Q)uit\n(Space) Show status\n(R)andomize health and position\n");
            if (isServer)
            {
                Console.Write("Toggle (M)onster\nToggle (p)layer\n");
                Console.Write("Toggle (S)cope of player\n");
            }

            Packet p;

            while (true)
            {
                p = rakPeer.Receive();
                while (p != null)
                {
                    byte[] data = p.data;  // The access to data member had better reduce it. Copying occurs by this.
                    if (data[0] == RakNetBindings.ID_DISCONNECTION_NOTIFICATION || data[0] == RakNetBindings.ID_CONNECTION_LOST)
                    {
                        if (isServer)
                        {
                            Console.Write("Server connection lost.  Deleting objects\n");
                            if (monster != null)
                            {
                                monster.Dispose();
                            }
                            if (player != null)
                            {
                                player.Dispose();
                            }
                        }
                    }
                    rakPeer.DeallocatePacket(p);
                    p = rakPeer.Receive();
                }

                if (_kbhit() != 0)
                {
                    ch = Console.ReadKey(true).KeyChar;
                    if (ch == 'q' || ch == 'Q')
                    {
                        Console.Write("Quitting.\n");
                        break;
                    }
                    else if (ch == ' ')
                        ShowStatus(monster, player);
                    else if (ch == 'r' || ch == 'R')
                    {
                        if (player != null)
                        {
                            player.health = (int)RakNetBindings.randomMT();
                            player.position = (int)RakNetBindings.randomMT();

                            replicaManager.SignalSerializeNeeded(player.replica, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                        if (monster != null)
                        {
                            monster.health = (int)RakNetBindings.randomMT();
                            monster.position = (int)RakNetBindings.randomMT();

                            replicaManager.SignalSerializeNeeded(monster.replica, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                        }
                        Console.Write("Randomized player and monster health and position\n");
                        ShowStatus(monster, player);
                    }
                    else if (isServer)
                    {
                        if (ch == 'm' || ch == 'M')
                        {
                            if (monster == null)
                            {
                                Console.Write("Creating monster\n");
                                monster = new Monster();
                            }
                            else
                            {
                                monster.Dispose();
                                Console.Write("Deleted monster\n");
                                monster = null;
                            }
                        }
                        else if (ch == 'p' || ch == 'P')
                        {
                            if (player == null)
                            {
                                Console.Write("Creating player\n");
                                player = new Player();

                            }
                            else
                            {
                                player.Dispose();
                                Console.Write("Deleted player\n");
                                player = null;
                            }
                        }
                        else if (ch == 's' || ch == 'S')
                        {
                            if (player != null)
                            {
                                bool currentScope;
                                currentScope = replicaManager.IsInScope(player.replica, rakPeer.GetSystemAddressFromIndex(0));
                                if (currentScope == false)
                                    Console.Write("Setting scope for player to true for all remote systems.\n");
                                else
                                    Console.Write("Setting scope for player to false for all remote systems.\n");
                                replicaManager.SetScope(player.replica, !currentScope, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
                            }
                            else
                            {
                                Console.Write("No player to set scope for\n");
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(30);
            }

            if (monster != null)
                monster.Dispose();
            if (player != null)
                player.Dispose();
            RakNetworkFactory.DestroyRakPeerInterface(rakPeer);

            return 1;
        }

        static void ShowStatus(Monster monster, Player player)
        {
            Console.Write("\nSTATUS:\n");
            if (monster != null)
                Console.Write("Monster is at position {0} with health {1}\n", monster.position, monster.health);
            else
                Console.Write("There is no monster\n");

            if (player != null)
                Console.Write("Player is at position {0} with health {1}\n", player.position, player.health);
            else
                Console.Write("There is no player\n");

            if (isServer)
            {
                bool monsterInScope, playerInScope;
                if (monster != null)
                {
                    // Note that when using a member object, I pass that member object to the ReplicaManager functions
                    monsterInScope = replicaManager.IsInScope(monster.replica, rakPeer.GetSystemAddressFromIndex(0));

                    // If an object is in scope we will send memory updates for that object to that player.
                    if (monsterInScope)
                        Console.Write("Monster in scope to system 0.\n");
                    else
                        Console.Write("Monster NOT in scope to system 0.\nChanges to monster variables will not be sent to that system.\n");
                }
                if (player != null)
                {
                    // For regular inheritance I don't have to do that.
                    playerInScope = replicaManager.IsInScope(player.replica, rakPeer.GetSystemAddressFromIndex(0));

                    if (playerInScope)
                        Console.Write("Player in scope to system 0.\n");
                    else
                        Console.Write("Player NOT in scope to system 0.\nChanges to player variables will not be sent to that system.\n");
                }
            }
            Console.Write("\n");
        }

        [System.Runtime.InteropServices.DllImport("crtdll.dll")]
        public static extern int _kbhit();  // I do not want to use this.
    }
}
