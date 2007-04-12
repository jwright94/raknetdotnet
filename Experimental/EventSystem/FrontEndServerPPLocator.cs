using Castle.Core.Logging;
using Events;

namespace EventSystem
{
    internal sealed class FrontEndServerPPLocator : IProtocolProcessorLocator
    {
        private readonly IProtocolProcessor processor;

        public FrontEndServerPPLocator(EventHandlersOnFrontEndServer handlers, IDOManager dOManager)
        {
            EventFactoryOnFrontEndServer factory = new EventFactoryOnFrontEndServer();
            ILogger logger = LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor));
            processor = new ProtocolProcessor(ProtocolInfo.Instance.Name, factory, handlers, dOManager, logger);
        }

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}