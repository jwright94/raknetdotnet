using Events;

namespace EventSystem
{
    sealed class ClientPPLocator : IProtocolProcessorLocator
    {
        public ClientPPLocator(EventHandlersOnClient handlers)
        {
            EventFactoryOnClient factory = new EventFactoryOnClient();
            processor = new ProtocolProcessor("samename", factory, handlers, LightweightContainer.LogFactory.Create(typeof(ProtocolProcessor)));
        }
        private IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}