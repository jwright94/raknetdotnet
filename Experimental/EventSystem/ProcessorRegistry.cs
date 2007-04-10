using System.Collections.Generic;
using Castle.Core;
using RakNetDotNet;

namespace EventSystem
{
    [Singleton]
    internal sealed class ProcessorRegistry : IProcessorRegistry
    {
        public ProcessorRegistry()
        {
            processorsByRecipient = new Dictionary<RakPeerInterface, IDictionary<string, IProtocolProcessor>>();
        }

        public void Add(RakPeerInterface recipient, IProtocolProcessor processor)
        {
            IDictionary<string, IProtocolProcessor> processors;
            if (processorsByRecipient.TryGetValue(recipient, out processors))
            {
                processors.Add(processor.ProtocolName, processor);
            }
            else
            {
                processors = new Dictionary<string, IProtocolProcessor>();
                processors.Add(processor.ProtocolName, processor);
                processorsByRecipient.Add(recipient, processors);
            }
        }

        public void Remove(RakPeerInterface recipient, IProtocolProcessor processor)
        {
            IDictionary<string, IProtocolProcessor> processors;
            if (processorsByRecipient.TryGetValue(recipient, out processors))
            {
                processors.Remove(processor.ProtocolName);
            }
        }

        public IProtocolProcessor GetProcessor(RakPeerInterface recipient, string protocolName)
        {
            return processorsByRecipient[recipient][protocolName];
        }

        private IDictionary<RakPeerInterface, IDictionary<string, IProtocolProcessor>> processorsByRecipient;
    }
}