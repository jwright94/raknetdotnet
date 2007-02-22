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
        bool RunOnServer { get; }
        bool PerformBeforeConnectOnClient { get; }
    }

    abstract class AbstractEvent : IEvent
    {
        public abstract BitStream Stream { get; }
        public abstract void Perform();
        public abstract bool IsBroadcast { get; }
        public abstract bool IsTwoWay { get; }
        public abstract bool RunOnServer { get; }
        public virtual bool PerformBeforeConnectOnClient { get { return false; } }

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
        public static void SendEventToClient(RPCParameters _params)
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
                    if (_event.PerformBeforeConnectOnClient)
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
        public static void SendEventToServer(RPCParameters _params)
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

    sealed class ServerToClientEvent : AbstractEvent
    {
        public ServerToClientEvent(int eventId, uint _objId)
        {
            objId = _objId;
            eventStream = null;
            Id = eventId;
        }
        public ServerToClientEvent(BitStream source)
        {
            int eventId;
            source.Read(out eventId);
            Id = eventId;

            source.Read(out objId);

            source.Read(out x);
        }
        public void SetData(float _x)
        {
            Console.WriteLine("setting data. x = {0}", _x);

            x = _x;
        }
        #region Private Members
        uint objId;  // I want to use ulong.
        float x;     // position
        BitStream eventStream;
        #endregion
        #region AbstractEvent Methods
        public override BitStream Stream
        {
            get 
            {
                eventStream = new BitStream();

                eventStream.Write(Id);

                eventStream.Write(x);

                return eventStream;
            }
        }
        public override void Perform()
        {
            Console.WriteLine("ServerToClientEvent.Perform(): x = {0}, objId = {1}", x, objId);  // or delegate to facade
        }
        public override bool IsBroadcast
        {
            get { return true; }
        }
        public override bool IsTwoWay
        {
            get { return false; }
        }
        public override bool RunOnServer
        {
            get { return false; }
        }
        #endregion
    }

    sealed class ClientToServerEvent : AbstractEvent
    {
        public ClientToServerEvent(int eventId)
        {
            eventStream = null;
            Id = eventId;
        }
        public ClientToServerEvent(BitStream source)
        {
            eventStream = null;

            int eventId;

            source.Read(out eventId);

            Id = eventId;
        }
        #region Private Members
        BitStream eventStream;
        #endregion
        #region AbstractEvent Methods
        public override BitStream Stream
        {
            get 
            {
                eventStream = new BitStream();

                eventStream.Write(Id);

                return eventStream;
            }
        }
        public override void Perform()
        {
            Console.WriteLine("ClientToServerEvent.Perform()");  
            // if this event is new player event then
            // send back origin player
            // send out new event.
        }
        public override bool IsBroadcast
        {
            get { return false; }
        }
        public override bool IsTwoWay
        {
            get { return false; }
        }
        public override bool RunOnServer
        {
            get { return true; }
        }
        #endregion
    }

    sealed class TestConnectionEvent : AbstractEvent
    {
        public TestConnectionEvent(int uniqueId)
        {
            cameBackFromServer = false;

            Id = uniqueId;
        }
        public TestConnectionEvent(BitStream stream)
        {
            eventStream = stream;

            int eventId;
            eventStream.Read(out eventId);
            Id = eventId;

            eventStream.Read(out cameBackFromServer);
        }
        #region Private Members
        BitStream eventStream;
        bool cameBackFromServer;
        #endregion
        #region AbstractEvent Methods
        public override BitStream Stream
        {
            get 
            {
                eventStream = new BitStream();

                eventStream.Write(Id);
                eventStream.Write(cameBackFromServer);

                return eventStream;
            }
        }
        public override void Perform()
        {
            if (cameBackFromServer)
            {
                Console.WriteLine("performs on client");
            }
            else
            {
                Console.WriteLine("performs on server");
                cameBackFromServer = true;
            }
        }
        public override bool IsBroadcast
        {
            get { return false; }
        }
        public override bool IsTwoWay
        {
            get { return true; }
        }
        public override bool RunOnServer
        {
            get { return true; }
        }
        public override bool PerformBeforeConnectOnClient
        {
            get { return false; }
        }
        #endregion
    }

    sealed class SampleEventFactory : AbstractEventFactory
    {
        #region Ogre-like singleton implementation.
        static SampleEventFactory instance;
        public SampleEventFactory() 
        {  
            Debug.Assert(instance == null);
            instance = this;
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;
        }
        public static SampleEventFactory Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        public enum EventTypes
        { 
            SERVERTOCLIENT,
            CLIENTTOSERVER,
            TESTCONNECTION,
        }
        public IEvent CreateEvent(EventTypes eventType, uint objId)
        {
            IEvent _event = null;

            switch (eventType)
            {
                case EventTypes.SERVERTOCLIENT:
                    _event = new ServerToClientEvent((int)EventTypes.SERVERTOCLIENT, objId);
                    break;

                case EventTypes.CLIENTTOSERVER:
                    _event = new ClientToServerEvent((int)EventTypes.CLIENTTOSERVER);
                    break;

                //case EventTypes.TESTCONNECTION:  // NOTE - this type shuld be created externally.
                    
                default:
                    throw new NetworkException(
                        string.Format("Event type {0} not recognized by SampleFactory.CreateEvent()!", eventType));
            }

            StoreEvent(_event);
            return _event;
        }
        public void StoreExternallyCreatedEvent(IEvent _event)
        {
            StoreEvent(_event);
        }
        public override IEvent RecreateEvent(BitStream source)
        {
            Debug.Assert(source != null);

            IEvent _event;

            int ID;
            source.Read(out ID);
            EventTypes eventType = (EventTypes)ID;
            source.ResetReadPointer();

            switch (eventType)
            {
                case EventTypes.SERVERTOCLIENT:
                    _event = new ServerToClientEvent(source);
                    break;

                case EventTypes.CLIENTTOSERVER:
                    _event = new ClientToServerEvent(source);
                    break;

                case EventTypes.TESTCONNECTION:
                    _event = new TestConnectionEvent(source);
                    break;

                default:
                    throw new NetworkException(
                        string.Format("Event type {0} not recognized by SampleFactory.CreateEvent()!", ID));
            }

            return _event;
        }
    }

    interface IFrameListener
    {
        bool FrameStarted();
    }

    interface IKeyListener
    {
        bool KeyPressed(char key);
    }

    sealed class Root
    {
        #region Implementing popular pattern of singleton
        static readonly Root instance = new Root();
        Root() { }
        public static Root Instance { get { return instance; } }
        #endregion
        public void AddFrameListener(IFrameListener newListener)
        {
            frameListeners.Add(newListener);
        }
        public void RemoteFrameListener(IFrameListener oldListener)
        {
            frameListeners.Remove(oldListener);
        }
        public void AddKeyListener(IKeyListener newListener)
        {
            keyListeners.Add(newListener);
        }
        public void RemoveKeyListener(IKeyListener oldListener)
        {
            keyListeners.Remove(oldListener);
        }
        public void StartRendering()
        {
            while (true)
            {
                if (_kbhit() != 0)
                {
                    char key = Console.ReadKey(true).KeyChar;
                    foreach (IKeyListener keyListener in keyListeners)
                    {
                        keyListener.KeyPressed(key);
                    }
                }
                foreach (IFrameListener frameListener in frameListeners)
                {
                    if (!frameListener.FrameStarted())
                        return;
                }
                System.Threading.Thread.Sleep(1);
            }
        }
        ICollection<IFrameListener> frameListeners = new List<IFrameListener>();
        ICollection<IKeyListener> keyListeners = new List<IKeyListener>();

        [System.Runtime.InteropServices.DllImport("crtdll.dll")]
        public static extern int _kbhit();  // I do not want to use this.
    }

    sealed class GameManager : IKeyListener, IFrameListener
    {
        #region Ogre-like singleton implementation.
        static GameManager instance;
        public GameManager() 
        {  
            Debug.Assert(instance == null);
            instance = this;
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;

            // clean up all the states
            while (0 < states.Count)
            {
                Debug.Assert(states.Peek() != null);
                states.Peek().Exit();
                states.Pop();
            }
        }
        public static GameManager Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        public void Start(IGameState state)
        {
            log("starting...");

            Root.Instance.AddFrameListener(this);

            Root.Instance.AddKeyListener(this);

            log("pusing first state...");
            PushState(state);

            Root.Instance.StartRendering();
        }
        public void ChangeState(IGameState state)
        {
            Debug.Assert(state != null);

            log("changing to state: {0}", state.Name);

            // cleanup the current state
            if (0 < states.Count)
            {
                IGameState oldState = states.Peek();

                if (state.Name == oldState.Name) return;

                log("exiting current state: {0}", oldState.Name);
                oldState.Exit();
                states.Pop();
            }

            // store and init the new state
            log("entering new state: {0}", state.Name);
            states.Push(state);
            states.Peek().Enter();
        }
        public void PushState(IGameState state)
        {
            Debug.Assert(state != null);

            log("pushing state: {0}", state.Name);

            if (0 < states.Count)
            {
                IGameState oldState = states.Peek();
                log("pause current state: {0}", oldState.Name);
                oldState.Pause();
            }

            log("store and init the new state: ", state.Name);
            states.Push(state);
            states.Peek().Enter();
        }
        public void PopState()
        {
            log("popping state");

            // TODO - impl.
        }
        #region Private Members
        void log(string message)
        {
            Console.WriteLine(message);
        }
        void log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
        Stack<IGameState> states = new Stack<IGameState>();
        #endregion
        #region IFrameListener Members
        public bool FrameStarted()
        {
            return states.Peek().FrameStarted();
        }
        #endregion
        #region IKeyListener Members
        public bool KeyPressed(char key)
        {
            states.Peek().KeyPressed(key);
            return true;
        }
        #endregion
    }

    interface IGameState
    {
        string Name { get; }
        void Enter();
        void Exit();
        void Pause();
        void Resume();
        bool KeyPressed(char key);
        bool FrameStarted();
    }

    abstract class AbstractGameState : IGameState
    {
        public AbstractGameState(string _name)
        {
            name = _name;
        }
        public virtual string Name
        {
            get { return name; }
        }
        string name;
        #region Protected Members
        protected void ChangeState(IGameState state)
        {
            GameManager.Instance.ChangeState(state);
        }
        protected void PushState(IGameState state)
        {
            GameManager.Instance.PushState(state);
        }
        protected void PopState()
        {
            GameManager.Instance.PopState();
        }
        #endregion
        #region IGameState Members
        public abstract void Enter();
        public abstract void Exit();
        public abstract void Pause();
        public abstract void Resume();
        public abstract bool KeyPressed(char key);
        public abstract bool FrameStarted();
        #endregion
    }

    sealed class IntroState : AbstractGameState
    {
        #region Ogre-like singleton implementation.
        static IntroState instance;
        public IntroState() : base("Intro") 
        {  
            Debug.Assert(instance == null);
            instance = this;
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;
        }
        public static IntroState Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        #region IGameState Members
        public override void Enter()
        {
        }
        public override void Exit()
        {
        }
        public override void Pause()
        {
        }
        public override void Resume()
        {
        }
        public override bool KeyPressed(char key)
        {
            if (key == 'p')
            {
                ChangeState(PlayState.Instance);
            }
            return true;
        }
        public override bool FrameStarted()
        {
            return true;
        }
        #endregion
    }

    class PlayState : AbstractGameState
    {
        #region Ogre-like singleton implementation.
        static PlayState instance;
        public PlayState(ushort _clientPort, string _serverIP) : base("Play")
        {  
            Debug.Assert(instance == null);
            instance = this;

            clientPort = _clientPort;
            serverIP = _serverIP;
        }
        public void  Dispose()
        {
            Debug.Assert(instance != null);
 	        instance = null;
        }
        public static PlayState Instance
        {
            get 
            { 
                Debug.Assert(instance != null);
                return instance; 
            }
        }
        #endregion
        #region IGameState Members
        public override void Enter()
        {
            log("starting...");

            try
            {
                ConnectToServer();
            }
            catch (NetworkException e)
            {
                Console.WriteLine(e.ToString());
                ChangeState(IntroState.Instance);
                return;
            }
        }
        public override void Exit()
        {
            SampleEventFactory.Instance.Dispose();
            rpcCalls.Dispose();

            eventCenterClient.Dispose();
        }
        public override void Pause()
        {
        }
        public override void Resume()
        {
        }
        public override bool KeyPressed(char key)
        {
            return true;
        }
        public override bool FrameStarted()
        {
            Update(0.0f);  // hmm

            return true;
        }
        #endregion
        #region Protected Members
        protected void Update(float dt)
        {
            UpdateNetwork();

            EventCenterClient.Instance.Update();
        }
        protected void UpdateNetwork()
        {
            EventCenterClient.Instance.Update();
        }
        protected void ConnectToServer()
        {
            log("starting network...");

            new SampleEventFactory();
            rpcCalls = new RpcCalls();
            rpcCalls.Handler = SampleEventFactory.Instance;

            eventCenterClient = new EventCenterClient("client.xml");
            eventCenterClient.OverrideClientPort(clientPort);
            eventCenterClient.OverrideServerPort(serverIP);
            eventCenterClient.ConnectPlayer();

            log("client started...");
            
            // TODO - try connect
            // blocking?
        }
        #endregion
        #region Private Members
        void log(string message)
        {
            Console.WriteLine(message);
        }
        ushort clientPort;
        string serverIP;

        EventCenterClient eventCenterClient;
        RpcCalls rpcCalls;
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("(S)erver or (C)lient?");
            char key = Console.ReadKey(true).KeyChar;
            if (key == 's' || key == 'S')
                ServerMain(args);
            else
                ClientMain(args);
        }

        static void ClientMain(string[] args)
        {
            // TODO - parse options
            ushort clientPort = 20000;
            string serverIP = "127.0.0.1";

            GameManager game = new GameManager();
            IntroState intro = new IntroState();
            PlayState play = new PlayState(clientPort, serverIP);

            try
            {
                game.Start(intro);

                Console.WriteLine("All done!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured: {0}", e.ToString());
            }

            game.Dispose();
            intro.Dispose();
            play.Dispose();

            Console.WriteLine("Quiting...");
        }

        static void ServerMain(string[] args)
        {
            EventCenterServer server = new EventCenterServer("server.xml");
            RpcCalls rpcCalls = new RpcCalls();
            SampleEventFactory factory = new SampleEventFactory();
            rpcCalls.Handler = factory;

            server.Start();

            factory.Dispose();
            server.Dispose();
        }
    }
}
