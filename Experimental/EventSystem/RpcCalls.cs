using System;
using System.Diagnostics;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal delegate void ProcessEventDelegate(IComplecatedEvent _event);

    [Singleton]
    [Obsolete]
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
            RpcCalls instance = LightweightContainer.Resolve<RpcCalls>();
            IComplecatedEvent _event = instance.RecreateEvent(source);

            Debug.Assert(instance.ProcessEventOnClientSide != null);
            instance.ProcessEventOnClientSide(_event);

            instance.WipeEvent(_event);
        }

        public static void SendEventToServer(RPCParameters _params)
        {
            SystemAddress sender = _params.sender;

            BitStream source = new BitStream(_params, false);

            RpcCalls instance = LightweightContainer.Resolve<RpcCalls>();
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