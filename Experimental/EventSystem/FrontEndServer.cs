using Castle.Core;
using Castle.Core.Logging;
using Events;
using RakNetDotNet;

namespace EventSystem
{
    internal interface IActor
    {
    }

    internal sealed class ServerActor : IActor
    {
        private readonly DObject dObject;
        private readonly ILogger logger;
        private readonly IServerCommunicator communicator;
        private readonly EventHandlersOnServerActor handlers;

        private string color = "red";

        public ServerActor(DObject dObject, ILogger logger, IServerCommunicator communicator)
        {
            this.dObject = dObject;
            this.logger = logger;
            this.communicator = communicator;
            handlers = new EventHandlersOnServerActor();

            this.dObject.OnGetEvent += OnGetEvent;
            handlers.ChangeColorRequest += Handler_OnChangeColorRequest;
        }

        private void Handler_OnChangeColorRequest(ChangeColorRequest e)
        {
            logger.Debug("Received ChangeColorRequest on ServerActor. color = {0}", e.Color);

            color = e.Color;

            ChangeColor changeColor = new ChangeColor();
            changeColor.Color = color;
            changeColor.TargetOId = dObject.OId;

            communicator.Broadcast(changeColor);
        }

        private void OnGetEvent(IEvent e)
        {
            handlers.CallHandler(e);
        }
    }

    [Transient]
    internal sealed class FrontEndServer : IServer
    {
        private readonly IServerCommunicator communicator;
        private readonly ILogger logger;        
        private readonly int sleepTimer;
        private readonly IServerDOManager dOManager;

        private IDObject rootDObject;
        private EventHandlersOnFrontEndServer handlers;

        public FrontEndServer(IServerCommunicator communicator, ILogger logger, int sleepTimer, IServerDOManager dOManager)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
            this.dOManager = dOManager;

            // Create root DObject
            // TODO - DObject should have OnGetEvent property.
            DObject newObject = new DObject(dOManager);
            newObject.OnGetEvent += RootDObjectHandler;
            rootDObject = newObject;
            dOManager.RegisterObject(newObject);
        }

        public void Startup()
        {
            handlers = new EventHandlersOnFrontEndServer();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            handlers.LogOn += Handlers_OnLogOnRequest;

            communicator.ProcessorLocator = new FrontEndServerPPLocator(handlers, dOManager);  // inject manually
            communicator.Startup();
        }

        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        private void RootDObjectHandler(IEvent e)
        {
            handlers.CallHandler(e);
        }

        private void Handlers_OnConnectionTest(ConnectionTest e)
        {
            logger.Debug("Handlers_OnConnectionTest was called on FrontEndServer.");
            communicator.Broadcast(e);  // echo back.
        }

        private void Handlers_OnLogOnRequest(LogOnEvent e)
        {
            logger.Info("Got login request from client");
            
            DObject newObject = new DObject(dOManager);
            LogOnACK ackEvent = new LogOnACK();
            ackEvent.NewOId = dOManager.RegisterObject(newObject);
            communicator.SendEvent(e.Sender, ackEvent);

            // create actor for player
            ILogger newLogger = LightweightContainer.LogFactory.Create(typeof (ServerActor));
            ServerActor newActor = new ServerActor(newObject, newLogger, communicator);
            
            // TODO - Is this obsolete?
            TestDOEvent testDOEvent = new TestDOEvent();
            testDOEvent.TargetOId = 0;
            communicator.SendEvent(e.Sender, testDOEvent);
        }

        public void Update()
        {
            communicator.Update();
        }

        public void Shutdown()
        {
            communicator.Shutdown();
        }
    }
}