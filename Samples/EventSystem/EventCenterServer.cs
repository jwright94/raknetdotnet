using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

    sealed class EventCenterServer : IDisposable, IEventProcessor
    {
        #region Ogre-like singleton implementation.
        static EventCenterServer instance;
        public EventCenterServer(string configFile)
        {
            Debug.Assert(instance == null);
            instance = this;

            // TODO - Use xml reader
            name = "Zeus";

            rakServerInterface = RakNetworkFactory.GetRakPeerInterface();

            ushort allowedPlayers = 5;
            int threadSleepTimer = 0;
            ushort port = 6000;
            SocketDescriptor socketDescriptor = new SocketDescriptor(port, string.Empty);
            rakServerInterface.Startup(allowedPlayers, threadSleepTimer, new SocketDescriptor[] { socketDescriptor }, 1);
            rakServerInterface.SetMaximumIncomingConnections(allowedPlayers);

            rakServerInterface.RegisterAsRemoteProcedureCall("sendeventtoserver", typeof(RpcCalls).GetMethod("SendEventToServer"));
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;

            log("Shutting down server...");
            rakServerInterface.Shutdown(1);
            rakServerInterface.UnregisterAsRemoteProcedureCall("sendeventtoserver");
            RakNetworkFactory.DestroyRakPeerInterface(rakServerInterface);
            log("Completed.");
        }
        public static EventCenterServer Instance
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
            Debug.Assert(_event != null);

            if (_event.RunOnServer) _event.Perform();

            if (_event.IsTwoWay)
            {
                PacketPriority priority = PacketPriority.HIGH_PRIORITY;
                PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
                byte orderingChannel = 0;
                SystemAddress player = _event.OriginPlayer;
                uint shiftTimestamp = 0;
                string sendevent = "sendeventtoclient";

                bool broadcast = _event.IsBroadcast;

                bool result = EventCenterServer.Instance.rakServerInterface.RPC(
                    sendevent,
                    _event.Stream, priority, reliability, orderingChannel,
                    player, broadcast, shiftTimestamp,
                    RakNetBindings.UNASSIGNED_NETWORK_ID, null);

                if (false)
                {
                    if (!result)
                        log("could not send data to the client!");
                    else
                        log("sent data to the client...");
                }
            }
        }
        public void SendEvent(IEvent _event)
        {
            PacketPriority priority = PacketPriority.HIGH_PRIORITY;
            PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
            byte orderingChannel = 0;
            SystemAddress player = _event.OriginPlayer;
            uint shiftTimestamp = 0;

            bool broadcast = _event.IsBroadcast;

            log("sending an event: [{0}], broadcast = {1}", _event.ToString(), broadcast);

            bool result = EventCenterServer.Instance.ServerInterface.RPC(
                "sendeventtoclient",
                _event.Stream, priority, reliability, orderingChannel,
                player, broadcast, shiftTimestamp,
                RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            if (!result)
                log("could not send data to the client!");
            else
                log("send data to the client...");
        }
        public void Start()
        {
            log("running...");

            Packet packet = null;

            while (true)
            {
                packet = rakServerInterface.Receive();

                if (packet != null)
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
                        default:
                            log("Message with identifier {0} has arrived.", packetIdentifier);
                            break;
                    }

                    rakServerInterface.DeallocatePacket(packet);
                }
                else
                {
                    System.Threading.Thread.Sleep(1);
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
            Console.WriteLine("EventCenterServer> {0}", message);
        }
        void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }
        string name;
        RakPeerInterface rakServerInterface;
        #endregion
    }
}
