namespace EventSystem
{
    internal interface IEventHandlers
    {
        void CallHandler(IEvent e);
    }
}