namespace EventSystem
{
    internal interface IDOManager
    {
        IDObject GetObject(int oId);
        void PostEvent(IEvent e);
        void SendEvent(IEvent e);
    }
}