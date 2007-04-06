using System;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal sealed class ClientToServerEvent : AbstractEvent
    {
        public ClientToServerEvent(int eventId)
        {
            eventStream = null;
            Id = eventId;
        }

        public ClientToServerEvent(BitStream source)
        {
            eventStream = null;

            int eventId;

            source.Read(out eventId);

            Id = eventId;
        }

        #region Private Members

        private BitStream eventStream;

        #endregion

        #region AbstractEvent Methods

        public override BitStream Stream
        {
            get
            {
                eventStream = new BitStream();

                eventStream.Write(Id);

                return eventStream;
            }
        }

        public override void Perform()
        {
            Console.WriteLine("ClientToServerEvent.Perform()");
            // if this event is new player event then
            // send back origin player
            // send out new event.
        }

        public override bool IsBroadcast
        {
            get { return false; }
        }

        public override bool IsTwoWay
        {
            get { return false; }
        }

        public override bool RunOnServer
        {
            get { return true; }
        }

        #endregion
    }
}