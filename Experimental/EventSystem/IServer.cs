namespace EventSystem
{
    internal interface IServer
    {
        void Startup();
        void Update();
        void Shutdown();
        int SleepTimer { get; }
    }
}