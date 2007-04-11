namespace EventSystem
{
    internal  interface IClientDOManager : IDOManager
    {
        void StoreObject(IDObject dObject);

        void SendEvent(IEvent e);

        IClientCommunicator ClientCommunicator
        {
            get;
            set;
        }
    }
}