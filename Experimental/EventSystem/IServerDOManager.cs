using System;
using RakNetDotNet;

namespace EventSystem
{
    public interface IServerDoManager
    {
        void RegisterObject(IDObject dObject);
    }
}