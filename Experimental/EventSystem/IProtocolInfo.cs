namespace EventSystem
{
    internal interface IProtocolInfo
    {
        string Name { get; }
        uint MajorVersion { get; }
        uint MinorVersion { get; }
    }
}