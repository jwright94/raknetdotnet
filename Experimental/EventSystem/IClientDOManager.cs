using System;
using RakNetDotNet;

namespace EventSystem
{
    public interface IClientDoManager
    {
        void StoreObject(IDObject dObject);
    }
}