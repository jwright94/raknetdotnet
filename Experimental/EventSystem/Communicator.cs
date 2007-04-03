using System;
using System.Collections;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    [Transient]
    internal sealed class Communicator : ICommunicator
    {
        private readonly IDictionary props;
        private readonly IProcessorRegistry registry;
        private readonly IProtocolProcessor[] processors;
        private readonly ILogger logger;
        private readonly RakPeerInterface rakPeerInterface;
        private IRpcBinder binder;

        public Communicator(IDictionary props, IProcessorRegistry registry, IProtocolProcessor processor, ILogger logger)
            : this(props, registry, new IProtocolProcessor[] {processor}, logger)
        {
        }

        public Communicator(IDictionary props, IProcessorRegistry registry, IProtocolProcessor[] processors,
                            ILogger logger)
        {
            this.props = props;
            this.registry = registry;
            this.processors = processors;
            this.logger = logger;
            rakPeerInterface = RakNetworkFactory.GetRakPeerInterface();
        }

        public void Startup()
        {
            ConnectionGraph connectionGraphPlugin = RakNetworkFactory.GetConnectionGraph(); // TODO - Do Destroy?
            FullyConnectedMesh fullyConnectedMeshPlugin = new FullyConnectedMesh(); // TODO - Do Dispose?

            // Initialize the message handlers
            fullyConnectedMeshPlugin.Startup(string.Empty);
            rakPeerInterface.AttachPlugin(fullyConnectedMeshPlugin);
            rakPeerInterface.AttachPlugin(connectionGraphPlugin);

            // Initialize the peers
            ushort allowedPlayers = (ushort) props["allowedplayers"];
            int threadSleepTimer = (int) props["threadsleeptimer"];
            ushort port = (ushort) props["port"];
            SocketDescriptor socketDescriptor = new SocketDescriptor(port, string.Empty);
            rakPeerInterface.Startup(allowedPlayers, threadSleepTimer, new SocketDescriptor[] {socketDescriptor}, 1);
            rakPeerInterface.SetMaximumIncomingConnections(allowedPlayers);

            binder = new RpcBinder(rakPeerInterface, registry, processors);
            binder.Bind();
        }

        public void Update()
        {
            Packet packet = rakPeerInterface.Receive();
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

                rakPeerInterface.DeallocatePacket(packet);
                packet = rakPeerInterface.Receive();
            }
        }

        public void Shutdown()
        {
            binder.Unbind();
            rakPeerInterface.Shutdown(0);
        }

        public void SendEvent(string processorName, IEvent e, SystemAddress address)
        {
            PacketPriority priority = PacketPriority.HIGH_PRIORITY;
            PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
            byte orderingChannel = 0;
            uint shiftTimestamp = 0;

            logger.Debug("sending an event: [{0}]", e.ToString());

            // TODO: only broadcast
            //bool result = rakPeerInterface.RPC(
            //    processorName,
            //    e.Stream, priority, reliability, orderingChannel,
            //    address, false, shiftTimestamp,
            //    RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            bool result = rakPeerInterface.RPC(
                processorName,
                e.Stream, priority, reliability, orderingChannel,
                RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, shiftTimestamp,
                RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            if (!result)
                logger.Debug("could not send data to the server!");
            else
                logger.Debug("send data to the server...");
        }

        public EventHandlersType GetEventHandlers<EventHandlersType>(string processorName)
        {
            throw new NotImplementedException();
        }

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
                    break;
                default:
                    logger.Debug("Message with identifier {0} has arrived.", packetIdentifier);
                    break;
            }
        }

        public void Dispose()
        {
            RakNetworkFactory.DestroyRakPeerInterface(rakPeerInterface);
        }
    }
}