using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;
    using RakNetDotNet;
    using Castle.Core;

    [Singleton]
    sealed class SampleEventFactory : AbstractEventFactory
    {
        public enum EventTypes
        {
            SERVERTOCLIENT,
            CLIENTTOSERVER,
            TESTCONNECTION,
            TESTCONNECTION2,
        }
        public IEvent CreateEvent(EventTypes eventType, uint objId)
        {
            IEvent _event = null;

            switch (eventType)
            {
                case EventTypes.SERVERTOCLIENT:
                    _event = new ServerToClientEvent((int)EventTypes.SERVERTOCLIENT, objId);
                    break;

                case EventTypes.CLIENTTOSERVER:
                    _event = new ClientToServerEvent((int)EventTypes.CLIENTTOSERVER);
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
        public void StoreExternallyCreatedEvent(IEvent _event)
        {
            StoreEvent(_event);
        }
        public override IEvent RecreateEvent(BitStream source)
        {
            Debug.Assert(source != null);

            IEvent _event;

            int ID;
            source.Read(out ID);
            EventTypes eventType = (EventTypes)ID;
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

                default:
                    throw new NetworkException(
                        string.Format("Event type {0} not recognized by SampleFactory.CreateEvent()!", ID));
            }

            return _event;
        }
    }
}
