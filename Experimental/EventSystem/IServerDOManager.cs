namespace EventSystem
{
    internal interface IServerDOManager : IDOManager
    {
        int RegisterObject(IDObject dObject);
    }
}