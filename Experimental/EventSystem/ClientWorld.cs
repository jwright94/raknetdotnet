using System;
using System.Diagnostics;

namespace EventSystem
{
    internal sealed class ClientWorld : IDisposable
    {
        #region Ogre-like singleton implementation.

        private static ClientWorld instance;

        public ClientWorld()
        {
            Debug.Assert(instance == null);
            instance = this;
        }

        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }

        public static ClientWorld Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }

        #endregion

        public void TestConnectionWithServer()
        {
            SetTestReplyFromServer(false);

            IEvent _event = new TestConnectionEvent((int) SampleEventFactory.EventTypes.TESTCONNECTION);

            ServiceConfigurator.Resolve<SampleEventFactory>().StoreExternallyCreatedEvent(_event);
            EventCenterClient.Instance.ReportEvent(_event);
        }

        public void SetTestReplyFromServer(bool flag)
        {
            gotTestResponseFromServer = flag;
        }

        public bool GetTestResponseFromServer()
        {
            return gotTestResponseFromServer;
        }

        #region Private Members

        private bool gotTestResponseFromServer;

        #endregion
    }
}