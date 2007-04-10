using RakNetDotNet;

namespace EventSystem
{
    // TODO: Pass more information.
    internal interface IEventExceptionCallbacks
    {
        void OnUnregistedEvent(SystemAddress sender);
        void OnRanOffEndOfBitstream(SystemAddress sender);
    }
}