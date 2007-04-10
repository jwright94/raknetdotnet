using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    [Transient]
    sealed class ProtocolProcessor : IProtocolProcessor
    {
        private readonly string protocolName;
        private readonly IEventFactory factory;
        private readonly IEventHandlers handlers;
        private readonly ILogger logger;
        private IEventExceptionCallbacks callbacks;
        private IDOManager dOManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolName"></param>
        /// <param name="factory"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public ProtocolProcessor(string protocolName, IEventFactory factory, IEventHandlers handlers, IDOManager dOManager, ILogger logger)
        {
            this.protocolName = protocolName;
            this.factory = factory;
            this.handlers = handlers;
            this.logger = logger;
            this.dOManager = dOManager;
        }

        public IEventExceptionCallbacks Callbacks
        {
            get { return callbacks; }
            set { callbacks = value; }
        }

        public string ProtocolName
        {
            get { return protocolName; }
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