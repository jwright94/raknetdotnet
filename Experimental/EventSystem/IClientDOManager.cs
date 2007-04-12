namespace EventSystem
{
    internal interface IClientDOManager : IDOManager
    {
        void StoreObject(IDObject dObject);

        // TODO - DI container set undesirable instance automatically.
        IClientCommunicator ClientCommunicator { get; set; }
    }
}