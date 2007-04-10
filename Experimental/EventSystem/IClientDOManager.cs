namespace EventSystem
{
    public interface IClientDOManager : IDOManager
    {
        void StoreObject(IDObject dObject);
    }
}