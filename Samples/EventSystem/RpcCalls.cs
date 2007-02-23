using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

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

            EventCenterClient.Instance.ProcessEvent(_event);

            Instance.WipeEvent(_event);
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
        AbstractEventFactory factory;
    }
}
