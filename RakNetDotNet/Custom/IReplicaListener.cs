namespace RakNetDotNet
{
    public interface IReplicaListener
    {
        ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress);
        ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp);
        ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp);
        ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, SWIGTYPE_p_unsigned_int includeTimestamp);
        void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int includeTimestamp);
        ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int includeTimestamp);
        ReplicaReturnResult Serialize(SWIGTYPE_p_unsigned_int sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags);
    }

    public sealed class NullReplicaListener : IReplicaListener
    {
        private static readonly NullReplicaListener instance = new NullReplicaListener();
        private NullReplicaListener() { }
        public static NullReplicaListener Instance
        {
            get { return instance; }
        }

        #region IReplicaListener Members
        public ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, SWIGTYPE_p_unsigned_int includeTimestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int includeTimestamp) { }
        public ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int includeTimestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public ReplicaReturnResult Serialize(SWIGTYPE_p_unsigned_int sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        #endregion
    }
}
