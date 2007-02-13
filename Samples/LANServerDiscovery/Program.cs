using System;
using System.Collections.Generic;
using System.Text;

namespace LANServerDiscovery
{
    using RakNetDotNet;

    class Program
    {
        static int Main(string[] args)
        {
            // Pointers to the interfaces of our server and client.
	        // Note we can easily have both in the same program
	        RakPeerInterface client;
	        RakPeerInterface server;
	        bool b;
	        string str;
	        string serverPort, clientPort;
	        uint quitTime;
	        // Holds packets
	        Packet p;	

	        Console.Write("A client / server sample showing how clients can broadcast offline packets\n");
	        Console.Write("to find active servers.\n");
	        Console.Write("Difficulty: Beginner\n\n");

	        Console.Write("Instructions:\nRun one or more servers on the same port.\nRun a client and it will get pongs from those servers.\n");
	        Console.Write("Run as (s)erver or (c)lient?\n");
            str = Console.ReadLine();

	        if (str[0]=='s' || str[0]=='S')
	        {
		        client=null;
		        server=RakNetworkFactory.GetRakPeerInterface();
		        // A server
		        Console.Write("Enter the server port\n");
                serverPort = Console.ReadLine();
		        if (serverPort.Equals(string.Empty))
                    serverPort = "60001";

		        Console.Write("Starting server.\n");
		        // The server has to be started to respond to pings.
		        SocketDescriptor socketDescriptor = new SocketDescriptor(ushort.Parse(serverPort),string.Empty);
		        b = server.Startup(2, 30, new SocketDescriptor[] { socketDescriptor }, 1);
		        server.SetMaximumIncomingConnections(2);
		        if (b)
			        Console.Write("Server started, waiting for connections.\n");
		        else
		        { 
			        Console.Write("Server failed to start.  Terminating.\n");
                    Environment.Exit(1);
		        }
	        }
	        else
	        {
		        client=RakNetworkFactory.GetRakPeerInterface();
		        server=null;

		        // Get our input
		        Console.Write("Enter the client port to listen on, or 0\n");
                clientPort = Console.ReadLine();
		        if (clientPort.Equals(string.Empty))
                    clientPort = "60000";
		        Console.Write("Enter the port to ping\n");
                serverPort = Console.ReadLine();
		        if (serverPort.Equals(string.Empty))
                    serverPort = "60001";
		        SocketDescriptor socketDescriptor = new SocketDescriptor(ushort.Parse(clientPort),string.Empty);
                client.Startup(1, 30, new SocketDescriptor[] { socketDescriptor }, 1);

		        // Connecting the client is very simple.  0 means we don't care about
		        // a connectionValidationInteger, and false for low priority threads
		        // All 255's mean broadcast
		        client.Ping("255.255.255.255", ushort.Parse(serverPort), true);

		        Console.Write("Pinging\n");
	        }

	        Console.Write("How many seconds to run this sample for?\n");
            str = Console.ReadLine();
	        if (str.Equals(string.Empty))
	        {
		        Console.Write("Defaulting to 5 seconds\n");
		        quitTime = RakNetBindings.GetTime() + 5000;
	        }
	        else
		        quitTime = RakNetBindings.GetTime() + uint.Parse(str) * 1000;

	        // Loop for input
            while (RakNetBindings.GetTime() < quitTime)
	        {
		        if (server != null)
			        p = server.Receive();
		        else 
			        p = client.Receive();

		        if (p == null)
		        {
                    System.Threading.Thread.Sleep(30);
			        continue;
		        }
		        if (server != null)
			        server.DeallocatePacket(p);
		        else
		        {
                    BitStream inBitStream = new BitStream(p, false);
                    byte packetIdentifier;
                    inBitStream.Read(out packetIdentifier);
                    if (packetIdentifier == RakNetBindings.ID_PONG)
			        {
				        uint time;
                        inBitStream.Read(out time);
				        Console.Write("Got pong from {0} with time {1}\n", p.systemAddress.ToString(), RakNetBindings.GetTime() - time);
			        }
			        client.DeallocatePacket(p);
		        }

                System.Threading.Thread.Sleep(30);
	        }

	        // We're done with the network
	        if (server != null)
		        RakNetworkFactory.DestroyRakPeerInterface(server);
	        if (client != null)
		        RakNetworkFactory.DestroyRakPeerInterface(client);

	        return 0;
        }
    }
}
