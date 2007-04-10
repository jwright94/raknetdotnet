namespace EventSystem
{
    delegate void GenericEventHandler<T>(T t) where T : IEvent;
}
