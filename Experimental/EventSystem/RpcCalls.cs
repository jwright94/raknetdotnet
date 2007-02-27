using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

    delegate void ProcessEventDelegate(IEvent _event);

    sealed class RpcCalls : IDisposable
    {
        #region Ogre-like singleton implementation.
        static RpcCalls instance;
        public RpcCalls()
        {
            Debug.Assert(instance == null);
            instance = this;
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }
        public static RpcCalls Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
        public static void SendEventToClient(RPCParameters _params)
        {
            BitStream source = new BitStream(_params, false);
            IEvent _event = Instance.RecreateEvent(source);

            Debug.Assert(Instance.ProcessEventOnClientSide != null);
            Instance.ProcessEventOnClientSide(_event);

            Instance.WipeEvent(_event);
        }
        public static void SendEventToServer(RPCParameters _params)
        {
            SystemAddress sender = _params.sender;

            BitStream source = new BitStream(_params, false);

            IEvent _event = RpcCalls.Instance.RecreateEvent(source);
            if (false) Console.WriteLine("EventCenterServer> {0}", _event.ToString());
            _event.OriginPlayer = sender;
            Debug.Assert(Instance.ProcessEventOnServerSide != null);
            Instance.ProcessEventOnServerSide(_event);
        }
        public IEvent RecreateEvent(BitStream source)
        {
            return factory.RecreateEvent(source);
        }
        public void WipeEvent(IEvent _event)
        {
            factory.WipeEvent(_event);
        }
        public AbstractEventFactory Handler
        {
            set { factory = value; }
        }
        /// <summary>
        /// if on sever-side then null
        /// </summary>
        public event ProcessEventDelegate ProcessEventOnClientSide;
        /// <summary>
        /// if on client-side then null
        /// </summary>
        public event ProcessEventDelegate ProcessEventOnServerSide;
        AbstractEventFactory factory;
    }
}
