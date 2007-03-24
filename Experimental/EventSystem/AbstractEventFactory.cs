using System.Collections.Generic;
using RakNetDotNet;

namespace EventSystem
{
    internal abstract class AbstractEventFactory : IEventFactory
    {
        public ISimpleEvent RecreateSimpleEvent(BitStream source)
        {
            return null;
        }

        public abstract IEvent RecreateEvent(BitStream source);

        public void Reset()
        {
            counter = 0;
            storage.Clear();
        }

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

        #region Transient State

        private ulong counter = 0;
        private ICollection<IEvent> storage = new List<IEvent>();

        #endregion
    }
}