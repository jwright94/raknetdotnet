using Events;

namespace EventSystem
{
    internal sealed class ClientPPLocator : IProtocolProcessorLocator
    {
        public ClientPPLocator(EventHandlersOnClient handlers, IDOManager dOManager)
        {
            EventFactoryOnClient factory = new EventFactoryOnClient();
            processor = new ProtocolProcessor(ProtocolInfo.Instance.Name, factory, handlers, dOManager, LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor)));
        }

        private IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}