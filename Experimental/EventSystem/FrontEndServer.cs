using System;
using Castle.Core;
using Castle.Core.Logging;
using Events;
using RakNetDotNet;

namespace EventSystem
{
    interface IActor
    {
        
    }

    sealed class ServerActor : IActor
    {
        private readonly DObject dObject;
        private readonly ILogger logger;
        private readonly IServerCommunicator comm;
        private readonly EventHandlersOnServerActor localHandler;
        private string color = "red";
        public ServerActor(DObject dObject, ILogger logger, IServerCommunicator comm)
        {
            this.dObject = dObject;
            this.logger = logger;
            this.comm = comm;
            localHandler = new EventHandlersOnServerActor();
            this.dObject.OnGetEvent += OnGetEvent;

            localHandler.ChangeColorRequest += Handler_OnChangeColorRequest;
        }

        private void Handler_OnChangeColorRequest(ChangeColorRequest t)
        {
            logger.Debug("Received ChangeColorRequest on ServerActor. color = {0}", t.Color);
            color = t.Color;
            ChangeColor changeColor = new ChangeColor();
            changeColor.SetData(color);
            changeColor.TargetOId = dObject.OId;
            comm.Broadcast(changeColor);
        }

        private void OnGetEvent(IEvent e)
        {
            localHandler.CallHandler(e);
        }
    }

    [Transient]
    internal sealed class FrontEndServer : IServer
    {
        private readonly ILogger logger;
        private readonly IServerCommunicator communicator;
        private readonly int sleepTimer;
        private readonly IServerDOManager dOManager;
        private IDObject rootDObject;
        private EventHandlersOnFrontEndServer handlers;
        private SystemAddress targetAddress;

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
            //Create root DObject
            DObject obj = new DObject(dOManager);
            obj.OnGetEvent += RootDObjectHandler;
            rootDObject = obj;
            dOManager.RegisterObject(rootDObject);
        }

        public void Startup()
        {
            handlers = new EventHandlersOnFrontEndServer();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            handlers.LogOn += Handlers_OnLogOnRequest;
            communicator.ProcessorLocator = new FrontEndServerPPLocator(handlers, dOManager); // inject manually
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
            communicator.Broadcast(t); // echo back.
        }

        private void Handlers_OnLogOnRequest(LogOnEvent e)
        {
            logger.Info("Got login request from client");
            LogOnACK ackEvent = new LogOnACK();

            

            ackEvent.NewOid = dOManager.RegisterObject(new DObject(dOManager));
            communicator.SendEvent(e.Sender, ackEvent);
            targetAddress = e.Sender;

            //create actor for player
            ServerActor newActor = new ServerActor((DObject)dOManager.GetObject(ackEvent.NewOid), LightweightContainer.LogFactory.Create(typeof(ServerActor)), communicator);

            

            TestDOEvent newEvent = new TestDOEvent();
            newEvent.TargetOId = 0;
            communicator.SendEvent(targetAddress, newEvent);
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