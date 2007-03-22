using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EventSerializerGenerator
{
    sealed class CodeWriter : ICodeWriter
    {
        public CodeWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }
        public void BeginBlock(string format, params object[] arg)
        {
            WriteLine(format, arg);
            levelOfIndent++;
        }
        public void EndBlock(string format, params object[] arg)
        {
            levelOfIndent = Math.Max(0, levelOfIndent - 1);
            WriteLine(format, arg);
        }
        public void WriteLine(string format, params object[] arg)
        {
            InsertTabs();
            if (0 < arg.Length)
            {
                textWriter.WriteLine(format, arg);
            }
            else
            {
                textWriter.WriteLine(format);
            }
        }
        void InsertTabs()
        {
            for (int i = 0; i < levelOfIndent; i++)
            {
                textWriter.Write("\t");
            }
        }
        TextWriter textWriter;
        int levelOfIndent;
    }
}
