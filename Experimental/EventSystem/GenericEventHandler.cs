namespace EventSystem
{
    internal delegate void GenericEventHandler<T>(T t) where T : IEvent;
}