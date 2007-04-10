using Events;

namespace EventSystem
{
    sealed class FrontEndServerPPLocator : IProtocolProcessorLocator
    {
        public FrontEndServerPPLocator(EventHandlersOnFrontEndServer handlers, IDOManager dOManager)
        {
            EventFactoryOnFrontEndServer factory = new EventFactoryOnFrontEndServer();
            processor = new ProtocolProcessor(Events.ProtocolInfo.Instance.Name, factory, handlers, dOManager, LightweightContainer.LogFactory.Create(typeof(ProtocolProcessor)));
        }

        private readonly IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}