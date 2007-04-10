using System;
using RakNetDotNet;

namespace EventSystem
{
    public interface IServerDOManager : IDOManager
    {
        int RegisterObject(IDObject dObject);
    }
}