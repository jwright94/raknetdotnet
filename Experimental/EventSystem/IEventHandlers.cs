namespace EventSystem
{
    internal interface IEventHandlers
    {
        // TODO: Add calling context
        void CallHandler(IEvent e);
    }
}