namespace ProtocolGenerator.Generators
{
    // I use the composit pattern.
    internal interface IGenerator
    {
        void AddChildGenerator(IGenerator generator);
        void RemoveChildGenerator(IGenerator generator);
        void Write(ICodeWriter o);
    }
}