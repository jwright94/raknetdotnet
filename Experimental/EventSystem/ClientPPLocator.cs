using Events;

namespace EventSystem
{
    sealed class ClientPPLocator : IProtocolProcessorsLocator
    {
        public ClientPPLocator(EventHandlersOnClient handlers)
        {
            EventFactoryOnClient factory = new EventFactoryOnClient();
            ProtocolProcessor processor = new ProtocolProcessor("samename", factory, handlers, LightweightContainer.LogFactory.Create(typeof(ProtocolProcessor)));
            processors = new IProtocolProcessor[] {processor};
        }
        private IProtocolProcessor[] processors;

        public IProtocolProcessor[] Processors
        {
            get { return processors; }
        }
    }
}