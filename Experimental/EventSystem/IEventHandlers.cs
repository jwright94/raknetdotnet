namespace EventSystem
{
    internal interface IEventHandlers
    {
        void CallHandler(IEvent e);  // TODO: Add calling context
    }
}