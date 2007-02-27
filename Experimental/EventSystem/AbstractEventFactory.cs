using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    abstract class AbstractEventFactory : IEventFactory
    {
        public abstract IEvent RecreateEvent(BitStream source);
        public void WipeEvent(IEvent _event)
        {
            if (storage.Contains(_event))
            {
                storage.Remove(_event);
            }
        }
        protected void StoreEvent(IEvent _event)
        {
            ++counter;
            storage.Add(_event);
        }
        ulong counter = 0;
        ICollection<IEvent> storage = new List<IEvent>();
    }
}
