namespace EventSystem
{
    internal interface IProtocolProcessorLocator
    {
        IProtocolProcessor Processor { get; }
    }
}