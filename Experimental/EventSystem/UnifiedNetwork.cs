using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    // NS(UN) - GS(UN) - FS(UN, ECS)
    // ReportEvent -> ProcessEventOnServerSide -> SendEvent -> ProcessEventOnClientSide
    internal sealed class UN
    {
        public string Name
        {
            get { return ""; }
        }

        private void ReportEvent(IEvent _event)
        {
        } // unicast to connected server.
        private void SendEvent(IEvent _event)
        {
        } // unicast, broadcast to connected systems.
        private void ProcessEventOnClientSide(IEvent _event)
        {
        } // only do perform.
        private void ProcessEventOnServerSide(IEvent _event)
        {
        } // RunOnServer, TwoWay. echo back.
        private void ConnectNameService()
        {
        }

        private void Start()
        {
        }

        private void Update()
        {
        }
    }

    // Based on ECS
    // TODO - Query service port to name service.
    // TODO - Rename 'player' related methods.
    internal sealed class UnifiedNetwork : IDisposable
    {
        #region Ogre-like singleton implementation.

        private static UnifiedNetwork instance;

        public UnifiedNetwork(string configFile, IDictionary extendedProperties)
        {
            Debug.Assert(instance == null);
            instance = this;
            logger = ServiceConfigurator.LogFactory.Create(typeof (UnifiedNetwork));

            // TODO - Use xml reader
            name = "Zeus";
            isOnline = true;
            isConnected = false;

            if (isOnline)
            {
                bool isNS = (bool) extendedProperties["isNS"];
                if (isNS)
                    namingComponent = new NamingServerComponent(logger.CreateChildLogger("namingserver"));
                else
                    namingComponent = new NamingClientComponent(logger.CreateChildLogger("namingclient"));

                rakServerInterface = RakNetworkFactory.GetRakPeerInterface();
                ConnectionGraph connectionGraphPlugin = RakNetworkFactory.GetConnectionGraph(); // TODO - Do Destroy?
                FullyConnectedMesh fullyConnectedMeshPlugin = new FullyConnectedMesh(); // TODO - Do Dispose?

                // Initialize the message handlers
                fullyConnectedMeshPlugin.Startup(string.Empty);
                rakServerInterface.AttachPlugin(fullyConnectedMeshPlugin);
                rakServerInterface.AttachPlugin(connectionGraphPlugin);

                // Initialize the peers
                //ushort allowedPlayers = 5;
                ushort allowedPlayers = (ushort) extendedProperties["allowedPlayers"];
                int threadSleepTimer = 0;
                //ushort port = 6000;
                ushort port = (ushort) extendedProperties["port"];
                SocketDescriptor socketDescriptor = new SocketDescriptor(port, string.Empty);
                rakServerInterface.Startup(allowedPlayers, threadSleepTimer, new SocketDescriptor[] {socketDescriptor},
                                           1);
                namingComponent.OnStartup(rakServerInterface);
                rakServerInterface.SetMaximumIncomingConnections(allowedPlayers);

                rakServerInterface.RegisterAsRemoteProcedureCall("sendeventtoserver",
                                                                 typeof (RpcCalls).GetMethod("SendEventToServer"));
            }
        }

        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;

            if (isOnline)
            {
                logger.Debug("Shutting down server...");
                rakServerInterface.Shutdown(1);
                rakServerInterface.UnregisterAsRemoteProcedureCall("sendeventtoserver");
                RakNetworkFactory.DestroyRakPeerInterface(rakServerInterface);
                logger.Debug("Completed.");
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

                if (_event.RunOnServer) _event.Perform(); // TODO - Check isConnected

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

                logger.Debug("sending an event: [{0}], broadcast = {1}", _event.ToString(), broadcast);

                bool result = rakServerInterface.RPC(
                    sendevent,
                    _event.Stream, priority, reliability, orderingChannel,
                    player, broadcast, shiftTimestamp,
                    RakNetBindings.UNASSIGNED_NETWORK_ID, null);

                if (false)
                {
                    if (!result)
                        logger.Debug("could not send data to the server!");
                    else
                        logger.Debug("send data to the server...");
                }
            }
        }

        public bool ConnectPlayer(string ip, ushort serverPort)
        {
            if (isOnline)
            {
                logger.Debug("connecting to the server...");

                //int internalSleep = threadSleepTimierMS;
                //SocketDescriptor socketDescriptor = new SocketDescriptor(clientPort, string.Empty);

                //logger.Debug("starting client on port {0}", clientPort);
                logger.Debug("connecting to server on ip= {0}, port = {1}", ip, serverPort);

                //rakClientInterface.Startup(1, internalSleep, new SocketDescriptor[] { socketDescriptor }, 1);
                //bool success = rakClientInterface.Connect(ip, serverPort, string.Empty, 0);
                bool success = rakServerInterface.Connect(ip, serverPort, string.Empty, 0);

                if (success)
                {
                    logger.Debug("connected!");
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
                while (packet != null) // Process all incoming packets. Do we need to switch other thread ?
                {
                    //StringBuilder message = new StringBuilder("recieved Message from player ");
                    //message.Append(packet.systemAddress.ToString());
                    //message.AppendFormat(" from ip {0}", packet.systemAddress.binaryAddress);
                    //message.AppendFormat(" from port {0}", packet.systemAddress.port);

                    //BitStream stream = new BitStream(packet, false);
                    //byte packetIdentifier;
                    //stream.Read(out packetIdentifier);
                    //message.AppendFormat(", stream = [{0}]", packetIdentifier);

                    //if (true) logger.Debug(message.ToString());

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
                logger.Debug("running...");

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
                        Thread.Sleep(1);
                    }
                }
            }
        }

        public RakPeerInterface ServerInterface
        {
            get { return rakServerInterface; }
        }

        #region Private Members

        private void HandlePacket(Packet packet)
        {
            logger.Debug("received Message:");
            BitStream inBitStream = new BitStream(packet, false);
            byte packetIdentifier;
            inBitStream.Read(out packetIdentifier);
            switch (packetIdentifier)
            {
                case RakNetBindings.ID_REMOTE_DISCONNECTION_NOTIFICATION:
                    logger.Debug("Another client has disconnected.\n");
                    break;
                case RakNetBindings.ID_REMOTE_CONNECTION_LOST:
                    logger.Debug("Another client has lost the connection.\n");
                    break;
                case RakNetBindings.ID_REMOTE_NEW_INCOMING_CONNECTION:
                    logger.Debug("Another client has connected.\n");
                    break;
                case RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED:
                    logger.Debug("Our connection request has been accepted.");
                    namingComponent.OnConnectionRequestAccepted(rakServerInterface, packet);
                    break;
                case RakNetBindings.ID_NEW_INCOMING_CONNECTION:
                    logger.Debug("A connection is incoming.\n");
                    break;
                case RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS:
                    logger.Debug("The server is full.\n");
                    break;
                case RakNetBindings.ID_DISCONNECTION_NOTIFICATION:
                    logger.Debug("A client has disconnected.\n");
                    break;
                case RakNetBindings.ID_CONNECTION_LOST:
                    logger.Debug("A client lost the connection.\n");
                    break;
                    //case RakNetBindings.ID_RECEIVED_STATIC_DATA:
                    //    logger.Debug("Got static data.\n");
                    //    break;
                case RakNetBindings.ID_DATABASE_UNKNOWN_TABLE:
                    logger.Debug("ID_DATABASE_UNKNOWN_TABLE\n");
                    break;
                case RakNetBindings.ID_DATABASE_INCORRECT_PASSWORD:
                    logger.Debug("ID_DATABASE_INCORRECT_PASSWORD\n");
                    break;
                case RakNetBindings.ID_DATABASE_QUERY_REPLY:
                    logger.Debug("ID_DATABASE_QUERY_REPLY\n");
                    namingComponent.OnDatabaseQueryReply(rakServerInterface, packet);
                    break;
                default:
                    logger.Debug("Message with identifier {0} has arrived.", packetIdentifier);
                    break;
            }
        }

        private string name;
        private RakPeerInterface rakServerInterface;
        private INamingComponent namingComponent;

        #endregion

        #region ECC's Private Member

        private bool isOnline;
        private bool isConnected;

        #endregion

        #region Eternal State

        private readonly ILogger logger;

        #endregion

        // Set FCM plugin to RakPeerInterface.
        // Set LWD plugin to RakPeerInterface.
        // Add argument of service name to SendEvent, ReportEvent.
        // ...
    }
}