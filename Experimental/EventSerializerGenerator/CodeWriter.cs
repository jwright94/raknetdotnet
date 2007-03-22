using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

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

    [TestFixture]
    public sealed class CodeWriteTestCase
    {
        [SetUp]
        public void SetUp()
        {
            textWriter = new StringWriter();
            codeWriter = new CodeWriter(textWriter);
        }
        [Test]
        public void Block()
        {
            Assert.AreEqual(textWriter.ToString(), "");
            codeWriter.WriteLine("// level 0");
            codeWriter.BeginBlock("public string ToString() {");
            codeWriter.WriteLine("return \"level 1\";");
            codeWriter.EndBlock("}");
            codeWriter.WriteLine("// level 0");
            StringBuilder expected = new StringBuilder();
            expected.AppendLine("// level 0");
            expected.AppendLine("public string ToString() {");
            expected.AppendLine("\treturn \"level 1\";");
            expected.AppendLine("}");
            expected.AppendLine("// level 0");
            Assert.AreEqual(expected.ToString(), textWriter.ToString());
        }
        TextWriter textWriter;
        ICodeWriter codeWriter;
    }
}
