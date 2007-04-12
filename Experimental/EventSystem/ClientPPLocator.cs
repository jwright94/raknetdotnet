using Castle.Core.Logging;
using Events;

namespace EventSystem
{
    internal sealed class ClientPPLocator : IProtocolProcessorLocator
    {
        private IProtocolProcessor processor;

        public ClientPPLocator(EventHandlersOnClient handlers, IDOManager dOManager)
        {
            EventFactoryOnClient factory = new EventFactoryOnClient();
            ILogger logger = LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor));
            processor = new ProtocolProcessor(ProtocolInfo.Instance.Name, factory, handlers, dOManager, logger);
        }

        public IProtocolProcessor Processor
        {
            get { return processor; }
        }
    }
}