using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;

    // NS(UN) - GS(UN) - FS(UN, ECS)
    // 
    sealed class UN
    {
        public string Name { get { return ""; } }
        void ReportEvent(IEvent _event) { }  // ReportEvent(string serviceName, IEvent _event) { }
        void SendEvent(IEvent _event) { }
        void ProcessEventOnClientSide(IEvent _event) { }  // only do perform.
        void ProcessEventOnServerSide(IEvent _event) { }  // RunOnServer, TwoWay.
        void ConnectNameService() { }
        void Start() { }
        void Update() { }
    }

    sealed class UnifiedNetwork : IDisposable
    {
        #region Ogre-like singleton implementation.
        static UnifiedNetwork instance;
        public UnifiedNetwork()
        {
            Debug.Assert(instance == null);
            instance = this;
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }
        public static UnifiedNetwork Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion


        #region Private Members
        void log(string message)
        {
            Console.WriteLine("UnifiedNetwork> {0}", message);
        }
        void log(string format, params object[] args)
        {
            log(string.Format(format, args));
        }
        string name;
        RakPeerInterface rakPeerInterface;
        #endregion
        // Set FCM plugin to RakPeerInterface.
        // Set LWD plugin to RakPeerInterface.
        // Add argument of service name to SendEvent, ReportEvent.
        // ...
    }
}
