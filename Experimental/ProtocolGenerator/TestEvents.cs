using System;
using ProtocolGenerator;
using RakNetDotNet;

namespace TestEvents
{
    [ProtocolInfo("TestProtocol", 1)]
    internal partial class ProtocolInfo
    {
    }

    [SiteOfHandling("Server")]
    internal partial class SimpleEvent
    {
        private string name;
        private int age;
        private SystemAddress[] systemAddresses;
        [NonSerialized]
        private int localValue;
    }
}