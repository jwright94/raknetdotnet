using EventSerializerGenerator;

namespace SampleEvents
{
    internal enum MyEnum
    {
        Foo,
        Bar,
        FooBar,
    }

    [SiteOfHandling("Server")]
    public partial class RegisterEvent
    {
        private string name;
        //SystemAddress[] systemAddresses;
        private byte serviceId;
        private int[] intArray;
    }
}

namespace AnotherSampleEvents
{
    [SiteOfHandling("Server")]
    [SiteOfHandling("Client")]
    public partial class OtherEvent
    {
    }
}