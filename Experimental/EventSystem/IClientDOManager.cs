namespace EventSystem
{
    internal interface IClientDOManager : IDOManager
    {
        void StoreObject(IDObject dObject);
        void InjectClientCommunicator(IClientCommunicator communicator);
    }
}