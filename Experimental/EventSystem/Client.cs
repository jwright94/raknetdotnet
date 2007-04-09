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
        private readonly int sleepTimer;
        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        public Client(IClientCommunicator communicator, ILogger logger, int sleepTimer)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
        }

        public void Startup()
        {
            EventHandlersOnClient handlers = new EventHandlersOnClient();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            communicator.ProcessorLocator = new ClientPPLocator(handlers);   // inject manually
            communicator.Startup();
            communicator.Connect();
        }

        private void Handlers_OnConnectionTest(ConnectionTest t)
        {
            logger.Debug("Handlers_OnConnectionTest was called on Client.");
        }

        public void Update()
        {
            communicator.Update();
            if(4000 < RakNetBindings.GetTime() - lastSent)
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