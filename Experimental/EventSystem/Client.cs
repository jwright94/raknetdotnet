using Castle.Core;
using Castle.Core.Logging;
using Events;
using RakNetDotNet;

namespace EventSystem
{
    /// <summary>
    /// test client
    /// </summary>
    [Transient]
    internal sealed class Client : IServer
    {
        private readonly ILogger logger;
        private readonly IClientCommunicator communicator;
        private uint lastSent;
        private readonly IClientDOManager dOManager;
        private EventHandlersOnClient handlers;
        private readonly int sleepTimer;
        private IDObject rootDObject;

        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        public Client(IClientCommunicator communicator, ILogger logger, int sleepTimer, IClientDOManager dOManager)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
            this.dOManager = dOManager;
            DObject obj = new DObject();
            obj.OnGetEvent += RootDObjectHandler;
            rootDObject = obj;
            dOManager.StoreObject(rootDObject);
        }

        public void Startup()
        {
            handlers = new EventHandlersOnClient();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            handlers.LogOnACK += Handlers_OnGetLogOnACK;
            communicator.ProcessorLocator = new ClientPPLocator(handlers, dOManager); // inject manually
            communicator.Startup();
            communicator.Connect();
            communicator.SendEvent(new LogOnEvent());
        }

        private void RootDObjectHandler(IEvent e)
        {
            handlers.CallHandler(e);
        }

        private void Handlers_OnConnectionTest(ConnectionTest t)
        {
            logger.Debug("Handlers_OnConnectionTest was called on Client.");
        }

        private void Handlers_OnGetLogOnACK(LogOnACK e)
        {
            logger.Info("Got ACK for login", e.NewOid);
            DObject newObject = new DObject();
            newObject.OId = e.NewOid;
            dOManager.StoreObject(newObject);
        }

        public void Update()
        {
            communicator.Update();
            if (4000 < RakNetBindings.GetTime() - lastSent)
            {
                ConnectionTest e = new ConnectionTest();
                communicator.SendEvent(e);
                lastSent = RakNetBindings.GetTime();
                logger.Debug("Sent ConnectionTest.");
            }
        }

        public void Shutdown()
        {
            communicator.Shutdown();
        }
    }
}