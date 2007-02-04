using System;
using System.Collections.Generic;
using System.Text;

namespace ReplicaManagerCS
{
    using RakNetDotNet;

    class Monster : IDisposable
    {
        public void Dispose()
        {
        }

        public ReplicaMember replica;

        public int position;
        public int health;
    }
}
