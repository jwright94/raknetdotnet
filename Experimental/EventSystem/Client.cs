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
        private readonly EventHandlersOnClientActor localHandler;
        private string color = "red";

        public ActorProxy(DObject dObject, ILogger logger)
        {
            this.dObject = dObject;
            this.logger = logger;
            localHandler = new EventHandlersOnClientActor();
            this.dObject.OnGetEvent += OnGetEvent;

            localHandler.ChangeColor += Handler_OnChangeColor;
        }

        public void ChangeColor()
        {
            ChangeColorRequest changeColorRequest = new ChangeColorRequest();
            changeColorRequest.TargetOId = dObject.OId;
            changeColorRequest.Color = "blue";
            dObject.SendEvent(changeColorRequest);
        }

        private void Handler_OnChangeColor(ChangeColor t)
        {
            logger.Debug("Received ChangeColorRequest on ServerActor. color = {0}", t.Color);
            color = t.Color;
        }

        private void OnGetEvent(IEvent e)
        {
            localHandler.CallHandler(e);
        }
    }

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
            dOManager.ClientCommunicator = communicator;
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

        private void ConnectionRequestAccepted()
        {
            logger.Debug("ConnectionRequestAccepted on Client");
            DObject obj = new DObject(dOManager);
            obj.OId = 0;
            obj.OnGetEvent += RootDObjectHandler;
            rootDObject = obj;
            dOManager.StoreObject(rootDObject);
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

        private Dictionary<int, ActorProxy> actors = new Dictionary<int, ActorProxy>();

        private void Handlers_OnGetLogOnACK(LogOnACK e)
        {
            logger.Info("Got ACK for login", e.NewOid);
            DObject newObject = new DObject(dOManager);
            newObject.OId = e.NewOid;
            dOManager.StoreObject(newObject);

            //create actor for player
            ActorProxy newActor = new ActorProxy((DObject)dOManager.GetObject(newObject.OId), LightweightContainer.LogFactory.Create(typeof (ActorProxy)));
            actors.Add(newObject.OId, newActor);
        }

        public void ChangeColor()
        {
            foreach (KeyValuePair<int, ActorProxy> pair in actors)
            {
                pair.Value.ChangeColor();
            }
        }

        public void Update()
        {
            communicator.Update();


            //if (4000 < RakNetBindings.GetTime() - lastSent)
            //{
            //    ConnectionTest e = new ConnectionTest();
            //    communicator.SendEvent(e);
            //    lastSent = RakNetBindings.GetTime();
            //    logger.Debug("Sent ConnectionTest.");
            //}
        }

        public void Shutdown()
        {
            communicator.Shutdown();
        }
    }
}