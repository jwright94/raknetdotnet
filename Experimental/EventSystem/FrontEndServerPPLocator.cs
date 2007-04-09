using Events;

namespace EventSystem
{
    sealed class FrontEndServerPPLocator : IProtocolProcessorLocator
    {
        public FrontEndServerPPLocator(EventHandlersOnFrontEndServer handlers)
        {
            EventFactoryOnFrontEndServer factory = new EventFactoryOnFrontEndServer();
            processor = new ProtocolProcessor("samename", factory, handlers, LightweightContainer.LogFactory.Create(typeof(ProtocolProcessor)));
        }

        private readonly IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}