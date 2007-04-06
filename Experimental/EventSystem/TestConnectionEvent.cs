using System;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal sealed class TestConnectionEvent : AbstractEvent
    {
        public TestConnectionEvent(int uniqueId)
        {
            cameBackFromServer = false;

            Id = uniqueId;
        }

        public TestConnectionEvent(BitStream stream)
        {
            eventStream = stream;

            int eventId;
            eventStream.Read(out eventId);
            Id = eventId;

            eventStream.Read(out cameBackFromServer);
        }

        #region Private Members

        private BitStream eventStream;
        private bool cameBackFromServer;

        #endregion

        #region AbstractEvent Methods

        public override BitStream Stream
        {
            get
            {
                eventStream = new BitStream();

                eventStream.Write(Id);
                eventStream.Write(cameBackFromServer);

                return eventStream;
            }
        }

        public override void Perform()
        {
            if (cameBackFromServer)
            {
                Console.WriteLine("performs on client");
                ClientWorld.Instance.SetTestReplyFromServer(true);
            }
            else
            {
                Console.WriteLine("performs on server");
                cameBackFromServer = true;
            }
        }

        public override bool IsBroadcast
        {
            get { return false; }
        }

        public override bool IsTwoWay
        {
            get { return true; }
        }

        public override bool RunOnServer
        {
            get { return true; }
        }

        public override bool PerformBeforeConnectOnClient
        {
            get { return false; }
        }

        #endregion
    }

    [Obsolete]
    internal sealed class TestConnectionEvent2 : AbstractEvent
    {
        public TestConnectionEvent2(int uniqueId)
        {
            cameBackFromServer = false;

            Id = uniqueId;
        }

        public TestConnectionEvent2(BitStream stream)
        {
            eventStream = stream;

            int eventId;
            eventStream.Read(out eventId);
            Id = eventId;

            eventStream.Read(out cameBackFromServer);
        }

        #region Private Members

        private BitStream eventStream;
        private bool cameBackFromServer;

        #endregion

        #region AbstractEvent Methods

        public override BitStream Stream
        {
            get
            {
                eventStream = new BitStream();

                eventStream.Write(Id);
                eventStream.Write(cameBackFromServer);

                return eventStream;
            }
        }

        public override void Perform()
        {
            if (cameBackFromServer)
            {
                Console.WriteLine("performs on client");
                //ClientWorld.Instance.SetTestReplyFromServer(true);
            }
            else
            {
                Console.WriteLine("performs on server");
                cameBackFromServer = true;
            }
        }

        public override bool IsBroadcast
        {
            //get { return false; }
            get { return true; }
        }

        public override bool IsTwoWay
        {
            //get { return true; }
            get { return false; }
        }

        public override bool RunOnServer
        {
            get { return true; }
        }

        public override bool PerformBeforeConnectOnClient
        {
            get { return false; }
        }

        #endregion
    }
}