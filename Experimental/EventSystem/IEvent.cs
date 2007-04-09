using System;
using RakNetDotNet;

namespace EventSystem
{
    internal interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        SystemAddress Sender { get; set; }
    }
}