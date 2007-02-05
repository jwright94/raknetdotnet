namespace RakNetDotNet
{
    public interface INetworkIDGeneratorListener
    {
        bool IsNetworkIDAuthority();
    }

    public interface IReplicaListener : INetworkIDGeneratorListener
    {
        ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress);
        ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp);
        ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp);
        ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, ref bool includeTimestamp);
        void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, ref bool includeTimestamp);
        ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, ref bool includeTimestamp);
        ReplicaReturnResult Serialize(ref bool sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags);
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
        public ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, ref bool includeTimestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, ref bool includeTimestamp) { }
        public ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, ref bool includeTimestamp) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        public ReplicaReturnResult Serialize(ref bool sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags) { return ReplicaReturnResult.REPLICA_CANCEL_PROCESS; }
        #endregion
        #region INetworkIDGeneratorListener Members
        public bool IsNetworkIDAuthority() { return false; }
        #endregion
    }
}
