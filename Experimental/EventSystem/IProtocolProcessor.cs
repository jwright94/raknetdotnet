using RakNetDotNet;

namespace EventSystem
{
    internal interface IProtocolProcessor
    {
        string ProtocolName { get; }
        void ProcessReceiveParams(RPCParameters _params);
    }
}