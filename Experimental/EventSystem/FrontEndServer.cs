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
        public int SleepTimer
        {
            get { return sleepTimer; }
        }

        public FrontEndServer(IServerCommunicator communicator, ILogger logger, int sleepTimer)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.sleepTimer = sleepTimer;
        }

        public void Startup()
        {
            EventHandlersOnFrontEndServer handlers = new EventHandlersOnFrontEndServer();
            handlers.ConnectionTest += Handlers_OnConnectionTest;
            communicator.ProcessorsLocator = new FrontEndServerPPLocator(handlers);   // inject manually
            communicator.Startup();
        }

        private void Handlers_OnConnectionTest(ConnectionTest t)
        {
            logger.Debug("Handlers_OnConnectionTest was called on FrontEndServer.");
            communicator.Broadcast("samename", t);  // echo back.
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