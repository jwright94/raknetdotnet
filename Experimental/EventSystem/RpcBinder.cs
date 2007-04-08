using RakNetDotNet;

namespace EventSystem
{
    internal sealed class RpcBinder : IRpcBinder
    {
        private readonly RakPeerInterface recipient;
        private readonly IProcessorRegistry registry;
        private readonly IProtocolProcessor[] processors;

        public RpcBinder(RakPeerInterface recipient, IProcessorRegistry registry, IProtocolProcessor processor)
            : this(recipient, registry, new IProtocolProcessor[] {processor})
        {
        }

        public RpcBinder(RakPeerInterface recipient, IProcessorRegistry registry, IProtocolProcessor[] processors)
        {
            this.recipient = recipient;
            this.registry = registry;
            this.processors = processors;
        }

        public void Bind()
        {
            foreach (IProtocolProcessor processor in processors)
            {
                registry.Add(recipient, processor);
                recipient.RegisterAsRemoteProcedureCall(processor.Name, GetType().GetMethod("Route"));
            }
        }

        public void Unbind()
        {
            foreach (IProtocolProcessor processor in processors)
            {
                registry.Remove(recipient, processor);
                recipient.UnregisterAsRemoteProcedureCall(processor.Name);
            }
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