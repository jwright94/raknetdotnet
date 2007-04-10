using Events;

namespace EventSystem
{
    internal sealed class FrontEndServerPPLocator : IProtocolProcessorLocator
    {
        public FrontEndServerPPLocator(EventHandlersOnFrontEndServer handlers, IDOManager dOManager)
        {
            EventFactoryOnFrontEndServer factory = new EventFactoryOnFrontEndServer();
            processor = new ProtocolProcessor(ProtocolInfo.Instance.Name, factory, handlers, dOManager, LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor)));
        }

        private readonly IProtocolProcessor processor;

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}