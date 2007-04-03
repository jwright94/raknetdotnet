using RakNetDotNet;

namespace EventSystem
{
    interface  IProtocolProcessor
    {
        string Name { get; }
        void ProcessReceiveParams(RPCParameters _params);
    }
}