using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        void Perform();
        bool isBroadcast();
        bool isTwoWay();
        SystemAddress OriginPlayer { get; set; }
        bool RunOnServer();
        bool PerformBeforeConnectOnClient();
    }

    abstract class AbstractEvent : IEvent
    {
        public abstract BitStream Stream { get; }
        public abstract void Perform();
        public abstract bool isBroadcast();
        public abstract bool isTwoWay();
        public abstract bool RunOnServer();
        public virtual bool PerformBeforeConnectOnClient() { return false; }

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

    interface IEventFactory 
    {
        IEvent RecreateEvent(BitStream source);
    }

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

    class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error) { }
    }

    sealed class RpcCalls
    {
        public IEvent RecreateEvent(BitStream source)
        {
            return factory.RecreateEvent(source);
        }
        public void WipeEvent(IEvent _event)
        {
            factory.WipeEvent(_event);
        }
        public AbstractEventFactory Handler
        {
            set { factory = value; }
        }
        AbstractEventFactory factory;
    }

    //static class RpcCalls
    //{
    //    public static void SendEventToClient(RPCParameters _params)
    //    {
    //    }
    //    public static void Add(SystemAddress, RpcCalls)
    //    {
    //    }
    //    public static void Remove(RpcCalls)
    //    {
    //    }
    //    IDictionary<SystemAddress, RpcCalls> dic = new Dictionary<SystemAddress, RpcCalls>();
    //}

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
