namespace EventSystem
{
    interface IProtocolProcessorsLocator
    {
        IProtocolProcessor[] Processors { get; }
    }
}