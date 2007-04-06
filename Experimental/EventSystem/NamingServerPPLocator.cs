using SampleEvents;

namespace EventSystem
{
    sealed class NamingServerPPLocator : IProtocolProcessorsLocator
    {
        public NamingServerPPLocator()
        {
            EventFactoryOnNamingServer factory = new EventFactoryOnNamingServer();
            EventHandlersOnNamingServer handlers = new EventHandlersOnNamingServer();
            ProtocolProcessor processor = new ProtocolProcessor("ns", factory, handlers, LightweightContainer.LogFactory.Create(typeof (ProtocolProcessor)));
            processors = new IProtocolProcessor[] { processor };
        }

        private readonly IProtocolProcessor[] processors;

        public IProtocolProcessor[] Processors
        {
            get { return processors; }
        }
    }
}