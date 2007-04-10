using Events;

namespace EventSystem
{
    sealed class ClientPPLocator : IProtocolProcessorLocator
    {
        public ClientPPLocator(EventHandlersOnClient handlers, IDOManager dOManager)
        {
            EventFactoryOnClient factory = new EventFactoryOnClient();
            processor = new ProtocolProcessor(Events.ProtocolInfo.Instance.Name, factory, handlers, dOManager, LightweightContainer.LogFactory.Create(typeof(ProtocolProcessor)));
        }
        private IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}