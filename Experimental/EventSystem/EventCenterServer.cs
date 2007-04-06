using System;
using System.Diagnostics;
using System.Threading;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal sealed class EventCenterServer : IDisposable
    {
        #region Ogre-like singleton implementation.

        private static EventCenterServer instance;

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
            rakServerInterface.Startup(allowedPlayers, threadSleepTimer, new SocketDescriptor[] {socketDescriptor}, 1);
            rakServerInterface.SetMaximumIncomingConnections(allowedPlayers);

            rakServerInterface.RegisterAsRemoteProcedureCall("sendeventtoserver",
                                                             typeof (RpcCalls).GetMethod("SendEventToServer"));
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

        public void ProcessEvent(IComplecatedEvent _event)
        {
            Debug.Assert(_event != null);

            if (_event.RunOnServer) _event.Perform();

            if (_event.IsTwoWay)
            {
                SendEvent(_event);
            }
        }

        public void SendEvent(IComplecatedEvent _event)
        {
            PacketPriority priority = PacketPriority.HIGH_PRIORITY;
            PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
            byte orderingChannel = 0;
            SystemAddress player = _event.OriginPlayer;
            uint shiftTimestamp = 0;
            string sendevent = "sendeventtoclient";

            bool broadcast = _event.IsBroadcast;

            log("sending an event: [{0}], broadcast = {1}", _event.ToString(), broadcast);

            bool result = Instance.ServerInterface.RPC(
                sendevent,
                _event.Stream, priority, reliability, orderingChannel,
                player, broadcast, shiftTimestamp,
                RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            if (false)
            {
                if (!result)
                    log("could not send data to the client!");
                else
                    log("send data to the client...");
            }
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
                    Thread.Sleep(1);
                }
            }
        }

        public RakPeerInterface ServerInterface
        {
            get { return rakServerInterface; }
        }

        #region Private Members

        private void log(string message)
        {
            Console.WriteLine("EventCenterServer> {0}", message);
        }

        private void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }

        private string name;
        private RakPeerInterface rakServerInterface;

        #endregion
    }
}