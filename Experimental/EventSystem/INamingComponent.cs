using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    interface INamingComponent
    {
        void OnStartup(RakPeerInterface peer);
        void OnConnectionRequestAccepted(RakPeerInterface peer, Packet packet);
        void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet);
    }
}
