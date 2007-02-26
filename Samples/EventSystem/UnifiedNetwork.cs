using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Collections;
    using System.Diagnostics;
    using RakNetDotNet;

    // NS(UN) - GS(UN) - FS(UN, ECS)
    // ReportEvent -> ProcessEventOnServerSide -> SendEvent -> ProcessEventOnClientSide
    sealed class UN
    {
        public string Name { get { return ""; } }
        void ReportEvent(IEvent _event) { }               // unicast to connected server.
        void SendEvent(IEvent _event) { }                 // unicast, broadcast to connected systems.
        void ProcessEventOnClientSide(IEvent _event) { }  // only do perform.
        void ProcessEventOnServerSide(IEvent _event) { }  // RunOnServer, TwoWay. echo back.
        void ConnectNameService() { }
        void Start() { }
        void Update() { }
    }

    // Based on ECS
    // TODO - Query service port to name service.
    // TODO - Rename 'player' related methods.
    sealed class UnifiedNetwork : IDisposable
    {
        #region Ogre-like singleton implementation.
        static UnifiedNetwork instance;
        public UnifiedNetwork(string configFile, IDictionary extendedProperties)
        {
            Debug.Assert(instance == null);
            instance = this;

            // TODO - Use xml reader
            name = "Zeus";
            isOnline = true;
            isConnected = false;

            if (isOnline)
            {
                bool isNS = (bool)extendedProperties["isNS"];
                if (isNS)
                    namingComponent = new NamingServerComponent();
                else
                    namingComponent = new NamingClientComponent();

                rakServerInterface = RakNetworkFactory.GetRakPeerInterface();
                ConnectionGraph connectionGraphPlugin = RakNetworkFactory.GetConnectionGraph();  // TODO - Do Destroy?
                FullyConnectedMesh fullyConnectedMeshPlugin = new FullyConnectedMesh();          // TODO - Do Dispose?

                // Initialize the message handlers
                fullyConnectedMeshPlugin.Startup(string.Empty);
                rakServerInterface.AttachPlugin(fullyConnectedMeshPlugin);
                rakServerInterface.AttachPlugin(connectionGraphPlugin);

                // Initialize the peers
                //ushort allowedPlayers = 5;
                ushort allowedPlayers = (ushort)extendedProperties["allowedPlayers"];
                int threadSleepTimer = 0;
                //ushort port = 6000;
                ushort port = (ushort)extendedProperties["port"];
                SocketDescriptor socketDescriptor = new SocketDescriptor(port, string.Empty);
                rakServerInterface.Startup(allowedPlayers, threadSleepTimer, new SocketDescriptor[] { socketDescriptor }, 1);
                namingComponent.OnStartup(rakServerInterface);
                rakServerInterface.SetMaximumIncomingConnections(allowedPlayers);

                rakServerInterface.RegisterAsRemoteProcedureCall("sendeventtoserver", typeof(RpcCalls).GetMethod("SendEventToServer"));
            }
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;

            if (isOnline)
            {
                log("Shutting down server...");
                rakServerInterface.Shutdown(1);
                rakServerInterface.UnregisterAsRemoteProcedureCall("sendeventtoserver");
                RakNetworkFactory.DestroyRakPeerInterface(rakServerInterface);
                log("Completed.");
            }
        }
        public static UnifiedNetwork Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
        public string Name
        {
            get { return name; }
        }
        public void ProcessEvent(IEvent _event)
        {
            if (isOnline)
            {
                Debug.Assert(_event != null);

                if (_event.RunOnServer) _event.Perform();  // TODO - Check isConnected

                if (_event.IsTwoWay)
                {
                    SendEvent(_event, _event.OriginPlayer);
                }
            }
        }
        public void SendEvent(IEvent _event, SystemAddress _player)
        {
            if (isOnline)
            {
                PacketPriority priority = PacketPriority.HIGH_PRIORITY;
                PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
                byte orderingChannel = 0;
                SystemAddress player = _player;
                uint shiftTimestamp = 0;
                //string sendevent = "sendeventtoclient";
                string sendevent = "sendeventtoserver";

                bool broadcast = _event.IsBroadcast;

                log("sending an event: [{0}], broadcast = {1}", _event.ToString(), broadcast);

                bool result = rakServerInterface.RPC(
                    sendevent,
                    _event.Stream, priority, reliability, orderingChannel,
                    player, broadcast, shiftTimestamp,
                    RakNetBindings.UNASSIGNED_NETWORK_ID, null);

                if (false)
                {
                    if (!result)
                        log("could not send data to the server!");
                    else
                        log("send data to the server...");
                }
            }
        }
        public bool ConnectPlayer(string ip, ushort serverPort)
        {
            if (isOnline)
            {
                log("connecting to the server...");

                //int internalSleep = threadSleepTimierMS;
                //SocketDescriptor socketDescriptor = new SocketDescriptor(clientPort, string.Empty);

                //log("starting client on port {0}", clientPort);
                log("connecting to server on ip= {0}, port = {1}", ip, serverPort);

                //rakClientInterface.Startup(1, internalSleep, new SocketDescriptor[] { socketDescriptor }, 1);
                //bool success = rakClientInterface.Connect(ip, serverPort, string.Empty, 0);
                bool success = rakServerInterface.Connect(ip, serverPort, string.Empty, 0);

                if (success)
                {
                    log("connected!");
                    isConnected = true;
                    return true;
                }
                else
                {
                    isOnline = false;
                    isConnected = false;
                    throw new NetworkException("Couldn't connect to server!");
                }
            }
            else
            {
                return false;
            }
        }
        public void Update()
        {
            if (IsOnline)
            {
                Packet packet = rakServerInterface.Receive();
                while (packet != null)  // Process all incoming packets. Do we need to switch other thread ?
                {
                    //StringBuilder message = new StringBuilder("recieved Message from player ");
                    //message.Append(packet.systemAddress.ToString());
                    //message.AppendFormat(" from ip {0}", packet.systemAddress.binaryAddress);
                    //message.AppendFormat(" from port {0}", packet.systemAddress.port);

                    //BitStream stream = new BitStream(packet, false);
                    //byte packetIdentifier;
                    //stream.Read(out packetIdentifier);
                    //message.AppendFormat(", stream = [{0}]", packetIdentifier);

                    //if (true) log(message.ToString());

                    HandlePacket(packet);

                    rakServerInterface.DeallocatePacket(packet);
                    packet = rakServerInterface.Receive();
                }
            }
        }
        public bool IsOnline
        {
            get { return isOnline; }
        }
        public void Start()
        {
            if (isOnline)
            {
                log("running...");

                Packet packet = null;

                while (true)
                {
                    packet = rakServerInterface.Receive();

                    if (packet != null)
                    {
                        HandlePacket(packet);

                        rakServerInterface.DeallocatePacket(packet);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
            }
        }
        public RakPeerInterface ServerInterface
        {
            get { return rakServerInterface; }
        }
        #region Private Members
        void log(string message)
        {
            Console.WriteLine("UnifiedNetwork> {0}", message);
        }
        void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }
        void HandlePacket(Packet packet)
        {
            log("received Message:");
            BitStream inBitStream = new BitStream(packet, false);
            byte packetIdentifier;
            inBitStream.Read(out packetIdentifier);
            switch (packetIdentifier)
            {
                case RakNetBindings.ID_REMOTE_DISCONNECTION_NOTIFICATION:
                    log("Another client has disconnected.\n");
                    break;
                case RakNetBindings.ID_REMOTE_CONNECTION_LOST:
                    log("Another client has lost the connection.\n");
                    break;
                case RakNetBindings.ID_REMOTE_NEW_INCOMING_CONNECTION:
                    log("Another client has connected.\n");
                    break;
                case RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED:
                    log("Our connection request has been accepted.");
                    break;
                case RakNetBindings.ID_NEW_INCOMING_CONNECTION:
                    log("A connection is incoming.\n");
                    break;
                case RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS:
                    log("The server is full.\n");
                    break;
                case RakNetBindings.ID_DISCONNECTION_NOTIFICATION:
                    log("A client has disconnected.\n");
                    break;
                case RakNetBindings.ID_CONNECTION_LOST:
                    log("A client lost the connection.\n");
                    break;
                //case RakNetBindings.ID_RECEIVED_STATIC_DATA:
                //    log("Got static data.\n");
                //    break;
                case RakNetBindings.ID_DATABASE_UNKNOWN_TABLE:
                    log("ID_DATABASE_UNKNOWN_TABLE\n");
                    break;
                case RakNetBindings.ID_DATABASE_INCORRECT_PASSWORD:
                    log("ID_DATABASE_INCORRECT_PASSWORD\n");
                    break;
                case RakNetBindings.ID_DATABASE_QUERY_REPLY:
                    log("ID_DATABASE_QUERY_REPLY\n");
                    namingComponent.OnDatabaseQueryReply(rakServerInterface, packet);
                    break;
                default:
                    log("Message with identifier {0} has arrived.", packetIdentifier);
                    break;
            }
        }
        string name;
        RakPeerInterface rakServerInterface;
        INamingComponent namingComponent;
        #endregion
        #region ECC's Private Member
        bool isOnline;
        bool isConnected;
        #endregion

        // Set FCM plugin to RakPeerInterface.
        // Set LWD plugin to RakPeerInterface.
        // Add argument of service name to SendEvent, ReportEvent.
        // ...
    }

    #region Naming Components
    interface INamingComponent
    {
        void OnStartup(RakPeerInterface peer);
        void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet);
    }
    sealed class NamingClientComponent : INamingComponent
    {
        #region INamingComponent Members
        public void OnStartup(RakPeerInterface peer)
        {
            peer.AttachPlugin(databaseClient);
        }
        public void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet)
        {
            NamingComponentHelper.PrintIncomingTable(packet);
        }
        #endregion
        LightweightDatabaseClient databaseClient = new LightweightDatabaseClient();
    }
    sealed class NamingServerComponent : INamingComponent
    {
        #region INamingComponent Members
        public void OnStartup(RakPeerInterface peer)
        {
            peer.AttachPlugin(databaseServer);

            string tableName = "Services";
            Table table = databaseServer.AddTable(tableName, true, true, true, string.Empty, string.Empty, string.Empty, true, true, true, true, true);
            if (table != null)
            {
                Console.Write("Table {0} created.\n", tableName);
                table.AddColumn("Name", Table.ColumnType.STRING);
            }
        }
        public void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet)
        {
            NamingComponentHelper.PrintIncomingTable(packet);
        }
        #endregion
        LightweightDatabaseServer databaseServer = new LightweightDatabaseServer();
    }
    sealed class NamingComponentHelper
    {
        public static bool PrintIncomingTable(Packet packet)
        {
            byte[] data = packet.data;
            Console.Write("Incoming table:\n");
            Table table = new Table();
            byte[] serializedTable = new byte[data.Length - sizeof(byte)];
            Array.Copy(data, sizeof(byte), serializedTable, 0, data.Length - sizeof(byte));  // ugly copy
            if (TableSerializer.DeserializeTable(serializedTable, (uint)serializedTable.Length, table))
            {
                TableRowPage cur = table.GetListHead();
                int i;

                Console.Write("Columns:\n");
                for (i = 0; i < table.GetColumns().Size(); i++)
                {
                    Console.Write("{0}. {1} : ", i + 1, table.GetColumns()[i].columnName);
                    if (table.GetColumns()[i].columnType == Table.ColumnType.BINARY)
                        Console.Write("BINARY");
                    else if (table.GetColumns()[i].columnType == Table.ColumnType.NUMERIC)
                        Console.Write("NUMERIC");
                    else
                        Console.Write("STRING");
                    Console.Write("\n");
                }
                if (cur != null)
                    Console.Write("Rows:\n");
                else
                    Console.Write("Table has no rows.\n");
                while (cur != null)
                {
                    for (i = 0; i < cur.size; i++)
                    {
                        StringBuilder sb = new StringBuilder(256);
                        table.PrintRow(sb, sb.Capacity, ',', true, cur.GetData(i));
                        Console.Write("RowID {0}: {1}\n", cur.GetKey(i), sb.ToString());
                    }
                    cur = cur.next;
                }
                return true;
            }
            else
            {
                Console.Write("Deserialization of table failed.\n");
                return false;
            }
        }
    }
    #endregion
}
