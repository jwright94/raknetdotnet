using RakNetDotNet;

namespace EventSystem
{
    internal sealed class RpcBinder : IRpcBinder
    {
        private readonly RakPeerInterface recipient;
        private readonly IProcessorRegistry registry;
        private readonly IProtocolProcessor processor;

        public RpcBinder(RakPeerInterface recipient, IProcessorRegistry registry, IProtocolProcessor processor)
        {
            this.recipient = recipient;
            this.registry = registry;
            this.processor = processor;
        }

        public void Bind()
        {
            registry.Add(recipient, processor);
            recipient.RegisterAsRemoteProcedureCall(processor.ProtocolName, GetType().GetMethod("Route"));
        }

        public void Unbind()
        {
            registry.Remove(recipient, processor);
            recipient.UnregisterAsRemoteProcedureCall(processor.ProtocolName);
        }

        public static void Route(RPCParameters _params)
        {
            IProcessorRegistry registry = LightweightContainer.Resolve<IProcessorRegistry>();
            // TODO - If client send illegal protocol name then server crushed.
            IProtocolProcessor processor = registry.GetProcessor(_params.recipient, _params.functionName);
            processor.ProcessReceiveParams(_params);
        }
    }
}