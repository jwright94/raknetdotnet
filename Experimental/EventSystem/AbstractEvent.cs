using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    abstract class AbstractEvent : IEvent
    {
        public abstract BitStream Stream { get; }
        public abstract void Perform();
        public abstract bool IsBroadcast { get; }
        public abstract bool IsTwoWay { get; }
        public abstract bool RunOnServer { get; }
        public virtual bool PerformBeforeConnectOnClient { get { return false; } }

        public int Id
        {
            get { return id; }
            protected set { id = value; }
        }
        public SystemAddress OriginPlayer
        {
            get { return originPlayer; }
            set { originPlayer = value; }
        }

        int id;
        SystemAddress originPlayer = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;
    }
}
