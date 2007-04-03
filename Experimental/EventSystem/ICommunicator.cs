namespace EventSystem
{
    interface ICommunicator
    {
        void Startup();
        void Update();
        void Shutdown();
    }
}