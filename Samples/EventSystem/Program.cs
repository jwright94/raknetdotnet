using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;
    using System.Diagnostics;

    interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        void Perform();
        bool IsBroadcast { get; }
        bool IsTwoWay { get; }
        SystemAddress OriginPlayer { get; set; }
        bool RunOnServer();
        bool PerformBeforeConnectOnClient();
    }

    abstract class AbstractEvent : IEvent
    {
        public abstract BitStream Stream { get; }
        public abstract void Perform();
        public abstract bool IsBroadcast { get; }
        public abstract bool IsTwoWay { get; }
        public abstract bool RunOnServer();
        public virtual bool PerformBeforeConnectOnClient() { return false; }

        public int Id
        {
            get { return id; }
            protected set { id = value; }
        }
        public SystemAddress OriginPlayer
        {
            get { return originPlayer; }
            set { originPlayer = value; }
        }

        int id;
        SystemAddress originPlayer = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;
    }

    interface IEventFactory 
    {
        IEvent RecreateEvent(BitStream source);
    }

    abstract class AbstractEventFactory : IEventFactory
    {
        public abstract IEvent RecreateEvent(BitStream source);
        public void WipeEvent(IEvent _event)
        {
            if (storage.Contains(_event))
            {
                storage.Remove(_event);
            }
        }
        protected void StoreEvent(IEvent _event)
        {
            ++counter;
            storage.Add(_event);
        }
        ulong counter = 0;
        ICollection<IEvent> storage = new List<IEvent>();
    }

    class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error) { }
    }

    sealed class RpcCalls : IDisposable
    {
        #region Ogre-like singleton implementation.
        static RpcCalls instance;
        public RpcCalls() 
        {  
            Debug.Assert(instance == null);
            instance = this;
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;
        }
        public static RpcCalls Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        static void SendEventToClient(RPCParameters _params)
        {
            BitStream source = new BitStream(_params, false);
            IEvent _event = Instance.RecreateEvent(source);

            EventCenterClient.Instance.ProcessEvent(_event);
            
            Instance.WipeEvent(_event);
        }
        public IEvent RecreateEvent(BitStream source)
        {
            return factory.RecreateEvent(source);
        }
        public void WipeEvent(IEvent _event)
        {
            factory.WipeEvent(_event);
        }
        public AbstractEventFactory Handler
        {
            set { factory = value; }
        }
        AbstractEventFactory factory;
    }

    sealed class EventCenterClient
    {
        #region Ogre-like singleton implementation.
        static EventCenterClient instance;
        public EventCenterClient(string xmlFile) 
        {  
            Debug.Assert(instance == null);
            instance = this;

            Load(xmlFile);
            isConnected = false;

            if (isOnline)
            {
                rakClientInterface = RakNetworkFactory.GetRakPeerInterface();
                rakClientInterface.RegisterAsRemoteProcedureCall("sendeventtoclient", typeof(RpcCalls).GetMethod("SendEventToClient"));
            }
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;

            if (IsOnline)
            {
                log("Closing connection...");
                rakClientInterface.Shutdown(10);
                rakClientInterface.UnregisterAsRemoteProcedureCall("sendeventtoclient");
                RakNetworkFactory.DestroyRakPeerInterface(rakClientInterface);
                log("Closed.");
            }
        }
        public static EventCenterClient Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        public void OverrideClientPort(ushort newPort)
        {
            clientPort = newPort;
            log("Overwrote client port = {0}", clientPort);
        }
        public void OverrideServerPort(string serverIP)
        {
            ipString = serverIP;
            log("Overwrote server IP = {0}", ipString);
        }
        public void ReportEvent(IEvent _event)
        {
            if (isOnline)
            {
                PacketPriority priority = PacketPriority.HIGH_PRIORITY;
                PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
                byte orderingChannel = 0;
                uint shiftTimestamp = 0;

                if (false) log("sending event: [{0}] ...", _event.ToString());

                bool success = rakClientInterface.RPC(
                    "sendeventtoserver", _event.Stream,
                    priority, reliability, orderingChannel,
                    RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true,
                    shiftTimestamp, RakNetBindings.UNASSIGNED_NETWORK_ID, null);

                if (!success)
                    log("Unable to send event to server.");
                else
                    log("Event was sent.");
            }
            else
            {
                log("not connected");
            }
        }
        public void ProcessEvent(IEvent _event)
        {
            if (isOnline)
            {
                if (isConnected)
                    _event.Perform();
                else
                {
                    if (_event.PerformBeforeConnectOnClient())
                        _event.Perform();
                }
            }
        }
        public bool ConnectPlayer()
        {
            if (isOnline)
                return ConnectPlayer(ipString, serverPort);
            else
                return false;
        }
        public bool ConnectPlayer(string ip)
        {
            if (isOnline)
                return ConnectPlayer(ip, serverPort);
            else
                return false;
        }
        public bool ConnectPlayer(string ip, ushort serverPort)
        {
            if (isOnline)
            {
                log("connecting to the server...");

                int internalSleep = threadSleepTimierMS;
                SocketDescriptor socketDescriptor = new SocketDescriptor(clientPort, string.Empty);

                log("starting client on port {0}", clientPort);
                log("connecting to server on ip= {0}, port = {1}", ip, serverPort);

                rakClientInterface.Startup(1, internalSleep, new SocketDescriptor[] { socketDescriptor }, 1);
                bool success = rakClientInterface.Connect(ip, serverPort, string.Empty, 0);

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
                Packet packet = rakClientInterface.Receive();
                while (packet != null)
                {
                    StringBuilder message = new StringBuilder("recieved Message from player ");
                    message.Append(packet.systemAddress.ToString());
                    message.AppendFormat(" from ip {0}", packet.systemAddress.binaryAddress);
                    message.AppendFormat(" from port {0}", packet.systemAddress.port);

                    BitStream stream = new BitStream(packet, false);
                    int eventType;  // int?
                    stream.Read(out eventType);
                    message.AppendFormat(", stream = [{0}]", eventType);

                    if (false) log(message.ToString());

                    rakClientInterface.DeallocatePacket(packet);
                    packet = rakClientInterface.Receive();
                }
            }
        }
        public bool IsOnline
        {
            get { return isOnline; }
        }
        #region Private Members
        void Load(string xmlFile)
        {
            // TODO - Use xml reader.
            ipString = "127.0.0.1";
            isOnline = true;
            serverPort = 6000;
            clientPort = 20000;
            threadSleepTimierMS = 0;
        }
        void log(string message)
        {
            Console.WriteLine(message);
        }
        void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }
        RakPeerInterface rakClientInterface;
        string ipString;
        ushort serverPort;
        ushort clientPort;
        int threadSleepTimierMS;
        bool isOnline;
        bool isConnected;
        #endregion
    }

    sealed class EventCenterServer
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

            rakServerInterface.RegisterAsRemoteProcedureCall("sendeventtoserver", typeof(EventCenterServer).GetMethod("SendEventToServer"));
        }
        public void  Dispose()
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
        static void SendEventToServer(RPCParameters _params)
        {
            SystemAddress sender = _params.sender;

            BitStream source = new BitStream(_params, false);

            IEvent _event = RpcCalls.Instance.RecreateEvent(source);
            if (false) Console.WriteLine("EventCenterServer> {0}", _event.ToString());
            _event.OriginPlayer = sender;
            EventCenterServer.Instance.ProcessEvent(_event);
        }
        public string Name
        {
            get { return name; }
        }
        public void ProcessEvent(IEvent _event)
        {
            Debug.Assert(_event != null);

            if (_event.RunOnServer()) _event.Perform();

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

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
