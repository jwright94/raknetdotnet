using System;
using RakNetDotNet;

namespace EventSystem
{
    public interface IServerDoManager
    {
        IDObject GetObject(int oId);
        void RegisterObject(IDObject dObject);
        void PostEvent(IEvent e);
    }
}