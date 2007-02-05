namespace RakNetDotNet
{
    public sealed class ReplicaMember : ReplicaBoolMarshalAsUInt
    {
        private IReplicaListener Listener
        {
            get 
            {
                IReplicaListener listener = (IReplicaListener)Parent;
                if (listener != null)
                {
                    return listener;
                }
                else
                {
                    return NullReplicaListener.Instance;
                }
            }
        }

        public ReplicaMember() { }
        
        public ReplicaMember(IReplicaListener listener) 
        {
            Parent = listener;
        }

        #region Replica Members
        public override ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress) 
        {
            return Listener.Deserialize(inBitStream, timestamp, lastDeserializeTime, systemAddress); 
        }
        public override ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            return Listener.ReceiveDestruction(inBitStream, systemAddress, timestamp);
        }
        public override ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            return Listener.ReceiveScopeChange(inBitStream, systemAddress, timestamp);
        }
        public override ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, ref bool includeTimestamp)
        {
            return Listener.SendConstruction(currentTime, systemAddress, outBitStream, ref includeTimestamp);
        }
        public override void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, ref bool includeTimestamp)
        {
            Listener.SendDestruction(outBitStream, systemAddress, ref includeTimestamp);
        }
        public override ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, ref bool includeTimestamp)
        {
            return Listener.SendScopeChange(inScope, outBitStream, currentTime, systemAddress, ref includeTimestamp);
        }
        public override ReplicaReturnResult Serialize(ref bool sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags)
        {
            return Listener.Serialize(ref sendTimestamp, outBitStream, lastSendTime, ref priority, ref reliability, currentTime, systemAddress, flags);
        }
        #endregion
        #region NetworkIDGenerator Members
        public override bool IsNetworkIDAuthority()
        {
            return Listener.IsNetworkIDAuthority();
        }
        public override bool RequiresSetParent()
        {
            return true;
        }
        #endregion
    }
}
