namespace EventSystem
{
    interface IServer
    {
        void Startup();
        void Update();
        void Shutdown();
        int SleepTimer { get; }
    }
}