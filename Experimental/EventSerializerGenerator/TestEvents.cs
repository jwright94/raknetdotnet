using System;
using System.Collections.Generic;
using System.Text;
using EventSerializerGenerator;

namespace TestEvents
{
    [SiteOfHandling("Server")]
    public partial class SimpleEvent
    {
        string name;
        int age;
    }
}
namespace TestEvents2
{
    [SiteOfHandling("Client")]
    public partial class SimpleEvent2
    {
        string name;
    }
}
