using RakNetDotNet;

namespace EventSystem
{
    // TODO: Pass more information.
    interface  IEventExceptionCallbacks
    {
        void OnUnregistedEvent(SystemAddress sender);
        void OnRanOffEndOfBitstream(SystemAddress sender);
    }
}