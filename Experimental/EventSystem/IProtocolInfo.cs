namespace EventSystem
{
    public interface IProtocolInfo
    {
        string Name { get; }
        int MajorVersion { get; }
        int MinorVersion { get; }
    }
}