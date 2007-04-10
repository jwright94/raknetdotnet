namespace EventSystem
{
    public interface IDOManager
    {
        IDObject GetObject(int oId);
        void PostEvent(IEvent e);
    }
}