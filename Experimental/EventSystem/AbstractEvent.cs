using RakNetDotNet;

namespace EventSystem
{
    internal abstract class AbstractEvent : IComplecatedEvent
    {
        public abstract BitStream Stream { get; }
        public abstract void Perform();
        public abstract bool IsBroadcast { get; }
        public abstract bool IsTwoWay { get; }
        public abstract bool RunOnServer { get; }

        public virtual bool PerformBeforeConnectOnClient
        {
            get { return false; }
        }

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

        private int id;
        private SystemAddress originPlayer = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;
    }
}