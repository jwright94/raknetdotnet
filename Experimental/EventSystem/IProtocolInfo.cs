namespace EventSystem
{
    public interface IProtocolInfo
    {
        string Name { get; }
        uint MajorVersion { get; }
        uint MinorVersion { get; }
    }
}