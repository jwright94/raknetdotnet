using System.Collections.Generic;
using RakNetDotNet;

namespace EventSystem
{
    internal abstract class AbstractEventFactory : IEventFactory
    {
        public IEvent RecreateSimpleEvent(BitStream source)
        {
            return null;
        }

        public abstract IComplecatedEvent RecreateEvent(BitStream source);

        public void Reset()
        {
            counter = 0;
            storage.Clear();
        }

        public void WipeEvent(IComplecatedEvent _event)
        {
            if (storage.Contains(_event))
            {
                storage.Remove(_event);
            }
        }

        protected void StoreEvent(IComplecatedEvent _event)
        {
            ++counter;
            storage.Add(_event);
        }

        #region Transient State

        private ulong counter = 0;
        private ICollection<IComplecatedEvent> storage = new List<IComplecatedEvent>();

        #endregion
    }
}