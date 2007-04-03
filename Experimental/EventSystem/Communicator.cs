using System;
using Castle.Core.Logging;

namespace EventSystem
{
    sealed class Communicator : ICommunicator
    {
        private readonly ILogger logger;

        public Communicator(ILogger logger)
        {
            this.logger = logger;
        }

        public void Startup()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}