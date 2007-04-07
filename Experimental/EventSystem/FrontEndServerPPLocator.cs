using Events;

namespace EventSystem
{
    sealed class FrontEndServerPPLocator : IProtocolProcessorsLocator
    {
        public FrontEndServerPPLocator(EventHandlersOnFrontEndServer handlers)
        {
            EventFactoryOnFrontEndServer factory = new EventFactoryOnFrontEndServer();
            ProtocolProcessor processor = new ProtocolProcessor("frontendserver", factory, handlers, LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor)));
            processors = new IProtocolProcessor[] { processor };
        }

        private readonly IProtocolProcessor[] processors;

        public IProtocolProcessor[] Processors
        {
            get { return processors; }
        }
    }
}