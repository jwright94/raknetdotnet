using RakNetDotNet;

namespace EventSystem
{
    internal interface INamingComponent
    {
        void OnStartup(RakPeerInterface peer);
        void OnConnectionRequestAccepted(RakPeerInterface peer, Packet packet);
        void OnDatabaseQueryReply(RakPeerInterface peer, Packet packet);
    }
}