using EventSerializerGenerator;

namespace TestEvents
{
    [SiteOfHandling("Server")]
    public partial class SimpleEvent
    {
        private string name;
        private int age;
    }
}

namespace TestEvents2
{
    [SiteOfHandling("Client")]
    public partial class SimpleEvent2
    {
        private string name;
    }
}