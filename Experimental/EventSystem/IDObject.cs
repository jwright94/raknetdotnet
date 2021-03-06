namespace EventSystem
{
    internal interface IDObject
    {
        int OId { get; set; }
        void HandleEvent(IEvent e);
        void PostEvent(IEvent e);
        void SendEvent(IEvent e);
    }
}