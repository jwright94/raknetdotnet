using System;
using System.Collections.Generic;
using System.Text;

namespace ReplicaManagerCS
{
    using RakNetDotNet;

    class Player : IDisposable, IReplicaListener
    {
        public Player()
        {
            position = 3;
            health = 4;

            replica = new ReplicaMember(this);

            if (Program.isServer)
                Program.replicaManager.Construct(replica, false, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);

            if (Program.isServer)
            {
                Program.replicaManager.DisableReplicaInterfaces(replica, RakNetBindings.REPLICA_RECEIVE_DESTRUCTION | RakNetBindings.REPLICA_RECEIVE_SCOPE_CHANGE);
            }
            else
            {
                Program.replicaManager.DisableReplicaInterfaces(replica, RakNetBindings.REPLICA_SEND_CONSTRUCTION | RakNetBindings.REPLICA_SEND_DESTRUCTION | RakNetBindings.REPLICA_SEND_SCOPE_CHANGE);
            }
        }

        public void Dispose()
        {
            Console.Write("Inside ~Player\n");

            Program.replicaManager.Destruct(replica, RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true);
            Program.replicaManager.DereferencePointer(replica);
            Program.player = null;
        }

        public ReplicaReturnResult SendConstruction(uint currentTime, SystemAddress systemAddress, BitStream outBitStream, SWIGTYPE_p_bool includeTimestamp)
        {
            outBitStream.Write("Player");

            Program.replicaManager.SetScope(replica, true, systemAddress, false);

            Console.Write("Sending monster to {0}:{1}\n", systemAddress.binaryAddress, systemAddress.port);

            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public void SendDestruction(BitStream outBitStream, SystemAddress systemAddress, SWIGTYPE_p_bool includeTimestamp)
        {
        }

        public ReplicaReturnResult ReceiveDestruction(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            Program.player.Dispose();

            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public ReplicaReturnResult SendScopeChange(bool inScope, BitStream outBitStream, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_bool includeTimestamp)
        {
            if (inScope)
                Console.Write("Sending scope change to true in Player\n");
            else
                Console.Write("Sending scope change to false in Player\n");

            outBitStream.Write(inScope);
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public ReplicaReturnResult ReceiveScopeChange(BitStream inBitStream, SystemAddress systemAddress, uint timestamp)
        {
            bool inScope;
            inBitStream.Read(out inScope);
            if (inScope)
                Console.Write("Received message that scope is now true in Player\n");
            else
                Console.Write("Received message that scope is now false in Player\n");
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public ReplicaReturnResult Serialize(SWIGTYPE_p_bool sendTimestamp, BitStream outBitStream, uint lastSendTime, ref PacketPriority priority, ref PacketReliability reliability, uint currentTime, SystemAddress systemAddress, SWIGTYPE_p_unsigned_int flags)
        {
            if (lastSendTime == 0)
                Console.Write("First call to Player::Serialize for {0}:{1}\n", systemAddress.binaryAddress, systemAddress.port);

            outBitStream.Write(position);
            outBitStream.Write(health);
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public ReplicaReturnResult Deserialize(BitStream inBitStream, uint timestamp, uint lastDeserializeTime, SystemAddress systemAddress)
        {
            if (lastDeserializeTime == 0)
                Console.Write("First call to Player::Deserialize\n");
            else
                Console.Write("Got Player::Deserialize\n");

            inBitStream.Read(out position);
            inBitStream.Read(out health);
            return ReplicaReturnResult.REPLICA_PROCESSING_DONE;
        }

        public bool IsNetworkIDAuthority()
        {
            return Program.isServer;
        }

        public ReplicaMember replica;

        public int position;
        public int health;
    }
}
