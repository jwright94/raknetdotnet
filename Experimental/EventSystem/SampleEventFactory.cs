using System;
using System.Diagnostics;
using Castle.Core;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal sealed class RegisterEvent : AbstractEvent
    {
        public RegisterEvent(int eventId)
        {
            eventStream = null;
            Id = eventId;
        }

        public RegisterEvent(BitStream source)
        {
            int eventId;
            source.Read(out eventId);
            Id = eventId;

            source.Read(out name);
        }

        public void SetData(string name, SystemAddress[] systemAddresses, byte serviceId)
        {
            this.name = name;
            this.systemAddresses = systemAddresses;
            this.serviceId = serviceId;
        }

        public override BitStream Stream
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override void Perform()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool IsBroadcast
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool IsTwoWay
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool RunOnServer
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #region Private Members

        private string name;
        private SystemAddress[] systemAddresses;
        private byte serviceId;
        private BitStream eventStream;

        #endregion
    }

    [Singleton]
    internal sealed class SampleEventFactory : AbstractEventFactory
    {
        [Obsolete]
        public enum EventTypes
        {
            SERVERTOCLIENT,
            CLIENTTOSERVER,
            TESTCONNECTION,
            TESTCONNECTION2,
            Register,
        }

        public IComplecatedEvent CreateEvent(EventTypes eventType, uint objId)
        {
            IComplecatedEvent _event = null;

            switch (eventType)
            {
                case EventTypes.SERVERTOCLIENT:
                    _event = new ServerToClientEvent((int) EventTypes.SERVERTOCLIENT, objId);
                    break;

                case EventTypes.CLIENTTOSERVER:
                    _event = new ClientToServerEvent((int) EventTypes.CLIENTTOSERVER);
                    break;

                    //case EventTypes.TESTCONNECTION:  // NOTE - this type shuld be created externally.

                    //case EventTypes.TESTCONNECTION2:  // NOTE - this type shuld be created externally.

                default:
                    throw new NetworkException(
                        string.Format("Event type {0} not recognized by SampleFactory.CreateEvent()!", eventType));
            }

            StoreEvent(_event);
            return _event;
        }

        public void StoreExternallyCreatedEvent(IComplecatedEvent _event)
        {
            StoreEvent(_event);
        }

        public override IComplecatedEvent RecreateEvent(BitStream source)
        {
            Debug.Assert(source != null);

            IComplecatedEvent _event;

            int ID;
            source.Read(out ID);
            EventTypes eventType = (EventTypes) ID;
            source.ResetReadPointer();

            switch (eventType)
            {
                case EventTypes.SERVERTOCLIENT:
                    _event = new ServerToClientEvent(source);
                    break;

                case EventTypes.CLIENTTOSERVER:
                    _event = new ClientToServerEvent(source);
                    break;

                case EventTypes.TESTCONNECTION:
                    _event = new TestConnectionEvent(source);
                    break;

                case EventTypes.TESTCONNECTION2:
                    _event = new TestConnectionEvent2(source);
                    break;

                case EventTypes.Register:
                    _event = new RegisterEvent(source);
                    break;

                default:
                    throw new NetworkException(
                        string.Format("Event type {0} not recognized by SampleFactory.CreateEvent()!", ID));
            }

            return _event;
        }
    }
}