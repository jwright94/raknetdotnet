using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    interface ICodeWriter
    {
        void BeginBlock(string format, params object[] arg);
        void EndBlock(string format, params object[] arg);
        void WriteLine(string format, params object[] arg);
    }
}
