using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    // I use the composit pattern.
    interface IGenerator
    {
        void AddChildGenerator(IGenerator generator);
        void RemoveChildGenerator(IGenerator generator);
        void Write(ICodeWriter o);
    }
}
