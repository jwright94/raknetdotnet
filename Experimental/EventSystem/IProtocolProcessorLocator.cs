namespace EventSystem
{
    interface IProtocolProcessorLocator
    {
        IProtocolProcessor Processor { get; }
    }
}