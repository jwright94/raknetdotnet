using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    sealed class ServerToClientEvent : AbstractEvent
    {
        public ServerToClientEvent(int eventId, uint _objId)
        {
            objId = _objId;
            eventStream = null;
            Id = eventId;
        }
        public ServerToClientEvent(BitStream source)
        {
            int eventId;
            source.Read(out eventId);
            Id = eventId;

            source.Read(out objId);

            source.Read(out x);
        }
        public void SetData(float _x)
        {
            Console.WriteLine("setting data. x = {0}", _x);

            x = _x;
        }
        #region Private Members
        uint objId;  // I want to use ulong.
        float x;     // position
        BitStream eventStream;
        #endregion
        #region AbstractEvent Methods
        public override BitStream Stream
        {
            get
            {
                eventStream = new BitStream();

                eventStream.Write(Id);

                eventStream.Write(x);

                return eventStream;
            }
        }
        public override void Perform()
        {
            Console.WriteLine("ServerToClientEvent.Perform(): x = {0}, objId = {1}", x, objId);  // or delegate to facade
        }
        public override bool IsBroadcast
        {
            get { return true; }
        }
        public override bool IsTwoWay
        {
            get { return false; }
        }
        public override bool RunOnServer
        {
            get { return false; }
        }
        #endregion
    }
}
