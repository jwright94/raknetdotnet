namespace EventSystem
{
    internal interface IEventHandlers
    {
        void CallHandler(ISimpleEvent e);
    }
}