using System;
using System.Collections.Generic;
using System.Text;

namespace ReplicaManagerCS
{
    using RakNetDotNet;

    class Program
    {
        static ReplicaManagerExt replicaManager = new ReplicaManagerExt();

        static void Main(string[] args)
        {
            RakPeerInterface rakPeer = RakNetworkFactory.GetRakPeerInterface();
            rakPeer.AttachPlugin(replicaManager);
            replicaManager.SetAutoParticipateNewConnections(true);
            replicaManager.SetAutoSerializeInScope(true);

        }
    }
}
