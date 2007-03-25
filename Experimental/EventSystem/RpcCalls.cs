using System.Diagnostics;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    // TODO: Pass more information.
    interface  IEventExceptionCallbacks
    {
        void OnUnregistedEvent(SystemAddress sender);
        void OnRanOffEndOfBitstream(SystemAddress sender);
    }

    interface  IProtocolProcessor
    {
        void ProcessReceiveParams(RPCParameters _params);
    }

    sealed class ProtocolProcessor : IProtocolProcessor
    {
        private readonly IEventFactory factory;
        private readonly IEventHandlers handlers;
        private readonly IEventExceptionCallbacks callbacks;
        private readonly ILogger logger;

        public ProtocolProcessor(IEventFactory factory, IEventHandlers handlers, ILogger logger) : this(factory, handlers, null, logger)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="handlers"></param>
        /// <param name="callbacks">Accept null.</param>
        /// <param name="logger"></param>
        public ProtocolProcessor(IEventFactory factory, IEventHandlers handlers, IEventExceptionCallbacks callbacks, ILogger logger)
        {
            this.factory = factory;
            this.handlers = handlers;
            this.callbacks = callbacks;
            this.logger = logger;
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
                if(callbacks != null)
                {
                    callbacks.OnRanOffEndOfBitstream(_params.sender);  // TODO: This is ad-hoc.
                }
            }
        }
    }

    internal delegate void ProcessEventDelegate(IComplecatedEvent _event);

    [Singleton]
    internal sealed class RpcCalls
    {
        public RpcCalls(AbstractEventFactory factory, ILogger logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public static void SendEventToClient(RPCParameters _params)
        {
            BitStream source = new BitStream(_params, false);
            RpcCalls instance = ServiceConfigurator.Resolve<RpcCalls>();
            IComplecatedEvent _event = instance.RecreateEvent(source);

            Debug.Assert(instance.ProcessEventOnClientSide != null);
            instance.ProcessEventOnClientSide(_event);

            instance.WipeEvent(_event);
        }

        public static void SendEventToServer(RPCParameters _params)
        {
            SystemAddress sender = _params.sender;

            BitStream source = new BitStream(_params, false);

            RpcCalls instance = ServiceConfigurator.Resolve<RpcCalls>();
            IComplecatedEvent _event = instance.RecreateEvent(source);
            instance.Logger.Debug("EventCenterServer> {0}", _event.ToString());
            _event.OriginPlayer = sender;
            Debug.Assert(instance.ProcessEventOnServerSide != null);
            instance.ProcessEventOnServerSide(_event);
        }

        public void Reset()
        {
            ProcessEventOnClientSide = null;
            ProcessEventOnServerSide = null;
        }

        public IComplecatedEvent RecreateEvent(BitStream source)
        {
            return factory.RecreateEvent(source);
        }

        public void WipeEvent(IComplecatedEvent _event)
        {
            factory.WipeEvent(_event);
        }

        public ILogger Logger
        {
            get { return logger; }
        }

        #region Transient State

        /// <summary>
        /// if on sever-side then null
        /// </summary>
        public event ProcessEventDelegate ProcessEventOnClientSide;

        /// <summary>
        /// if on client-side then null
        /// </summary>
        public event ProcessEventDelegate ProcessEventOnServerSide;

        #endregion

        #region Eternal State

        private readonly AbstractEventFactory factory;
        private readonly ILogger logger;

        #endregion
    }
}