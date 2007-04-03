using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    sealed class ProtocolProcessor : IProtocolProcessor
    {
        private readonly string name;
        private readonly IEventFactory factory;
        private readonly IEventHandlers handlers;
        private readonly ILogger logger;
        private IEventExceptionCallbacks callbacks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factory"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public ProtocolProcessor(string name, IEventFactory factory, IEventHandlers handlers, ILogger logger)
        {
            this.name = name;
            this.factory = factory;
            this.handlers = handlers;
            this.logger = logger;
        }

        public IEventExceptionCallbacks Callbacks
        {
            get { return callbacks; }
            set { callbacks = value; }
        }

        public string Name
        {
            get { return name; }
        }

        public void ProcessReceiveParams(RPCParameters _params)
        {
            BitStream source = new BitStream(_params, false);
            try
            {
                IEvent e = factory.RecreateSimpleEvent(source);
                e.Sender = _params.sender;
                handlers.CallHandler(e);
            }
            catch (NetworkException)  // TODO: Add new type of network exception. Call accurate callback.
            {
                logger.Warn("Ran off end of packet.");
                if(Callbacks != null)
                {
                    Callbacks.OnRanOffEndOfBitstream(_params.sender);  // TODO: This is ad-hoc.
                }
            }
        }
    }
}