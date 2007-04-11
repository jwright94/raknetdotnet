using System.Diagnostics;
using System.Reflection;
using Castle.Core.Logging;
using RakNetDotNet;
using System;

namespace EventSystem
{
    delegate void RakNetEventHandler();

    enum RakNetMessageId
    {
        RemoteDisconnectionNotification = RakNetBindings.ID_REMOTE_DISCONNECTION_NOTIFICATION,
        RemoteConnectionLost = RakNetBindings.ID_REMOTE_CONNECTION_LOST,
        RemoteNewIncomingConnection = RakNetBindings.ID_REMOTE_NEW_INCOMING_CONNECTION,
        ConnectionRequestAccepted = RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED,
        NewIncomingConnection = RakNetBindings.ID_NEW_INCOMING_CONNECTION,
        NoFreeIncomingConnections = RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS,
        DisconnectionNotification = RakNetBindings.ID_DISCONNECTION_NOTIFICATION,
        ConnectionLost = RakNetBindings.ID_CONNECTION_LOST,
    }

    /// <summary>
    /// for implementation
    /// </summary>
    internal sealed class CommunicatorModule
    {
        private readonly IProcessorRegistry registry;
        private readonly ILogger logger;
        private RakPeerInterface rakPeerInterface;
        private IProtocolProcessorLocator processorLocator;
        private IRpcBinder binder;

        public CommunicatorModule(IProcessorRegistry registry, ILogger logger)
        {
            this.registry = registry;
            this.logger = logger;
        }

        public IProtocolProcessorLocator ProcessorLocator
        {
            get { return processorLocator; }
            set { processorLocator = value; }
        }

        public RakPeerInterface RakPeerInterface
        {
            get { return rakPeerInterface; }
        }

        public void Startup(ushort maxConnections, int threadSleepTimer, ushort port)
        {
            Startup(maxConnections, threadSleepTimer, port, new PluginInterface[] {});
        }

        public void Startup(ushort maxConnections, int threadSleepTimer, ushort port, PluginInterface[] plugins)
        {
            rakPeerInterface = RakNetworkFactory.GetRakPeerInterface();

            foreach (PluginInterface plugin in plugins)
            {
                RakPeerInterface.AttachPlugin(plugin);
            }

            // Initialize the peer
            SocketDescriptor socketDescriptor = new SocketDescriptor(port, string.Empty);
            RakPeerInterface.Startup(maxConnections, threadSleepTimer, new SocketDescriptor[] {socketDescriptor}, 1);
            RakPeerInterface.SetMaximumIncomingConnections(maxConnections);

            binder = new RpcBinder(RakPeerInterface, registry, ProcessorLocator.Processor);
            binder.Bind();
        }

        public void Update()
        {
            try
            {
                Packet packet = RakPeerInterface.Receive();
                while (packet != null) // Process all incoming packets. Do we need to switch other thread ?
                {
                    HandlePacket(packet);
                    RakPeerInterface.DeallocatePacket(packet);
                    packet = RakPeerInterface.Receive();
                }
            }
            catch (TargetInvocationException e)
            {
                Debugger.Break();
                throw e.InnerException;
            }
        }

        public void Shutdown()
        {
            RakPeerInterface.Shutdown(0);
            binder.Unbind();
            RakNetworkFactory.DestroyRakPeerInterface(RakPeerInterface);
        }

        System.Collections.Generic.Dictionary<RakNetMessageId, RakNetEventHandler> rakNetHandlers = new System.Collections.Generic.Dictionary<RakNetMessageId, RakNetEventHandler>();

        public void RegisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            rakNetHandlers.Add(messageId, handler);
        }

        public void UnregisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            rakNetHandlers.Remove(messageId);
        }

        private void CallRakNetEventHandler(RakNetMessageId messageId)
        {
            RakNetEventHandler handler;
            if (rakNetHandlers.TryGetValue(messageId, out handler))
            {
                try
                {
                    handler.Invoke();
                }
                catch (TargetInvocationException e)
                {
                    Debugger.Break();
                    throw e.InnerException;
                }
            }
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
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_REMOTE_CONNECTION_LOST:
                    logger.Debug("Another client has lost the connection.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_REMOTE_NEW_INCOMING_CONNECTION:
                    logger.Debug("Another client has connected.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_CONNECTION_REQUEST_ACCEPTED:
                    logger.Debug("Our connection request has been accepted.");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_NEW_INCOMING_CONNECTION:
                    logger.Debug("A connection is incoming.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_NO_FREE_INCOMING_CONNECTIONS:
                    logger.Debug("The server is full.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_DISCONNECTION_NOTIFICATION:
                    logger.Debug("A client has disconnected.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
                case RakNetBindings.ID_CONNECTION_LOST:
                    logger.Debug("A client lost the connection.\n");
                    CallRakNetEventHandler((RakNetMessageId)packetIdentifier);
                    break;
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
    }
}