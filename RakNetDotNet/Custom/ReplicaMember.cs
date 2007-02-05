namespace RakNetDotNet
{
    public sealed class ReplicaMember : ReplicaBoolMarshalAsUInt
    {
        private IReplicaListener listener = NullReplicaListener.Instance;
        public IReplicaListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }

        public ReplicaMember() { }
        
        public ReplicaMember(IReplicaListener listener) 
        {
            this.listener = listener;
        }

        #region Replica Members
        public override ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress) 
        { 
            return listener.Deserialize(inBitStream, timestamp, lastDeserializeTime, systemAddress); 
        }
        public override ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            return listener.ReceiveDestruction(inBitStream, systemAddress, timestamp);
        }
        public override ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            return listener.ReceiveScopeChange(inBitStream, systemAddress, timestamp);
        }
        public override ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, ref bool includeTimestamp)
        {
            return listener.SendConstruction(currentTime, systemAddress, outBitStream, ref includeTimestamp);
        }
        public override void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, ref bool includeTimestamp)
        {
            listener.SendDestruction(outBitStream, systemAddress, ref includeTimestamp);
        }
        public override ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, ref bool includeTimestamp)
        {
            return listener.SendScopeChange(inScope, outBitStream, currentTime, systemAddress, ref includeTimestamp);
        }
        public override ReplicaReturnResult Serialize(ref bool sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags)
        {
            return listener.Serialize(ref sendTimestamp, outBitStream, lastSendTime, ref priority, ref reliability, currentTime, systemAddress, flags);
        }
        #endregion
        #region NetworkIDGenerator Members
        public override bool IsNetworkIDAuthority()
        {
            return listener.IsNetworkIDAuthority();
        }
        public override bool RequiresSetParent()
        {
            return true;
        }
        #endregion
    }
}
