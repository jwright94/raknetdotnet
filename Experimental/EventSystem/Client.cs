using System.Collections.Generic;
using Castle.Core;
using Castle.Core.Logging;
using Events;

namespace EventSystem
{
    internal sealed class ActorProxy : IActor
    {
        private readonly DObject dObject;
        private readonly ILogger logger;
        private readonly EventHandlersOnClientActor handlers;

        private string color = "red";

        public ActorProxy(DObject dObject, ILogger logger)
        {
            this.dObject = dObject;
            this.logger = logger;
            handlers = new EventHandlersOnClientActor();

            this.dObject.OnGetEvent += OnGetEvent;
            handlers.ChangeColor += Handler_OnChangeColor;
        }

        public void ChangeColor()
        {
            ChangeColorRequest changeColorRequest = new ChangeColorRequest();
            changeColorRequest.TargetOId = dObject.OId;
            changeColorRequest.Color = "blue";

            dObject.SendEvent(changeColorRequest);
        }

        private void Handler_OnChangeColor(ChangeColor e)
        {
            logger.Debug("Received ChangeColorRequest on ServerActor. color = {0}", e.Color);
            color = e.Color;
        }

        private void OnGetEvent(IEvent e)
        {
            handlers.CallHandler(e);
        }
    }

    /// <summary>
    /// test client
    /// </summary>
    [Transient]
    internal sealed class Client : IServer
    {
        private readonly IClientCommunicator communicator;
        private readonly ILogger logger;
        private readonly int sleepTimer;
        private readonly IClientDOManager dOManager;
        
        private EventHandlersOnClient handlers;
        private IDObject rootDObject;
        private IDictionary<int, ActorProxy> actors = new Dictionary<int, ActorProxy>();

        public Client(IClientCommunicator communicator, ILogger logger, int sleepTimer, IClientDOManager dOManager)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
            this.dOManager = dOManager;
            dOManager.ClientCommunicator = communicator;  // inject manually
        }

        public void Startup()
        {
            handlers = new EventHandlersOnClient();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            handlers.LogOnACK += Handlers_OnGetLogOnACK;

            communicator.ProcessorLocator = new ClientPPLocator(handlers, dOManager); // inject manually
            communicator.RegisterRakNetEventHandler(RakNetMessageId.ConnectionRequestAccepted, ConnectionRequestAccepted);
            communicator.Startup();
            communicator.Connect();
        }

        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        private void ConnectionRequestAccepted()
        {
            logger.Debug("ConnectionRequestAccepted on Client");

            // TODO - DObject should have OnGetEvent property.
            DObject newObject = new DObject(dOManager);
            newObject.OId = 0;
            newObject.OnGetEvent += RootDObjectHandler;
            rootDObject = newObject;
            dOManager.StoreObject(newObject);

            communicator.SendEvent(new LogOnEvent());
        }

        private void RootDObjectHandler(IEvent e)
        {
            handlers.CallHandler(e);
        }

        private void Handlers_OnConnectionTest(ConnectionTest e)
        {
            logger.Debug("Handlers_OnConnectionTest was called on Client.");
        }

        private void Handlers_OnGetLogOnACK(LogOnACK e)
        {
            logger.Info("Got ACK for login", e.NewOId);

            DObject newObject = new DObject(dOManager);
            newObject.OId = e.NewOId;
            dOManager.StoreObject(newObject);

            // create actor for player
            ActorProxy newActor = new ActorProxy(newObject, LightweightContainer.LogFactory.Create(typeof (ActorProxy)));
            actors.Add(newObject.OId, newActor);
        }

        public void ChangeColor()
        {
            foreach (ActorProxy actor in actors.Values)
            {
                actor.ChangeColor();
            }
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