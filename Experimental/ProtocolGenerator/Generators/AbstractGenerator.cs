using System.Collections.Generic;

namespace ProtocolGenerator.Generators
{
    internal abstract class AbstractGenerator : IGenerator
    {
        public void AddChildGenerator(IGenerator generator)
        {
            generators.Add(generator);
        }

        public void RemoveChildGenerator(IGenerator generator)
        {
            generators.Remove(generator);
        }

        public abstract void Write(ICodeWriter o);
        protected ICollection<IGenerator> generators = new List<IGenerator>();
    }
}