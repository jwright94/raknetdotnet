using RakNetDotNet;

namespace EventSystem
{
    interface  IProtocolProcessor
    {
        string ProtocolName { get; }
        void ProcessReceiveParams(RPCParameters _params);
    }
}