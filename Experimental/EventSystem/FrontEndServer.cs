using Castle.Core;
using Castle.Core.Logging;
using Events;

namespace EventSystem
{
    [Transient]
    internal sealed class FrontEndServer : IServer
    {
        private readonly ILogger logger;
        private readonly IServerCommunicator communicator;
        private readonly int sleepTimer;
        private readonly IServerDOManager dOManager;
        private IDObject rootDObject;
        private EventHandlersOnFrontEndServer handlers;
        private RakNetDotNet.SystemAddress targetAddress;

        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        public FrontEndServer(IServerCommunicator communicator, ILogger logger, int sleepTimer, IServerDOManager dOManager)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
            this.dOManager = dOManager;
            DObject obj = new DObject();            
            obj.OnGetEvent += RootDObjectHandler;
            rootDObject = obj;
            dOManager.RegisterObject(rootDObject);            
        }

        public void Startup()
        {
            handlers = new EventHandlersOnFrontEndServer();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            handlers.LogOn += Handlers_OnLogOnRequest;
            communicator.ProcessorLocator = new FrontEndServerPPLocator(handlers, dOManager);   // inject manually
            communicator.Startup();
        }

        private void RootDObjectHandler(IEvent e)
        {
            handlers.CallHandler(e);
        }

        private void Handlers_OnConnectionTest(ConnectionTest t)
        {
            logger.Debug("Handlers_OnConnectionTest was called on FrontEndServer.");
            ConnectionTest response = new ConnectionTest();
            communicator.Broadcast(t);  // echo back.
        }

        private void Handlers_OnLogOnRequest(LogOnEvent e)
        {
            logger.Info("Got login request from client");
            LogOnACK ackEvent = new LogOnACK();
            ackEvent.NewOid = dOManager.RegisterObject(new DObject());
            communicator.SendEvent(ackEvent, e.Sender);
            targetAddress = e.Sender;

            TestDOEvent newEvent = new TestDOEvent();
            newEvent.TargetOid = 0;
            communicator.SendEvent(newEvent, targetAddress);
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