using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    using System.Reflection;
    using System.CodeDom.Compiler;
    using Microsoft.CSharp;
    using System.IO;
    using System.Globalization;

    // CodeDom is complicated. I use a simpler method. It is Console.WriteLine.
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return 1;
            }

            string eventTemplatePath = args[0];
            Assembly templateAssembly = null;
            string[] referencedAssemblies = new string[] { "RakNetDotNet.dll" };

            if (!CompileExecutable(eventTemplatePath, referencedAssemblies, out templateAssembly))
            {
                return 1;
            }

            //p("using System;");
            //p("using System.Collections.Generic;");
            //p("using System.Text;");
            //p("using RakNetDotNet;");

            IDictionary<string, StringBuilder> namespaceClassNameHash = new Dictionary<string, StringBuilder>();

            foreach (Type t in templateAssembly.GetTypes())
            {
                //p("namespace {0} {{", t.Namespace);
                //p("\tpublic partial class {0} \n{{\n}}", t.Name);
                //p("}");
            }

            return 0;
        }

        static bool CompileExecutable(String sourceName, string[] referencedAssemblies, out Assembly compiledAssembly)
        {
            FileInfo sourceFile = new FileInfo(sourceName);
            CodeDomProvider provider = null;
            bool compileOk = false;
            compiledAssembly = null;

            if (!sourceFile.Exists)
            {
                Console.WriteLine("Source file not found.");
                return false;
            }

            // Select the code provider based on the input file extension.
            if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
            {
                provider = new Microsoft.CSharp.CSharpCodeProvider();
            }
            else if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".VB")
            {
                provider = new Microsoft.VisualBasic.VBCodeProvider();
            }
            else
            {
                Console.WriteLine("Source file must have a .cs or .vb extension");
                return false;
            }

            if (provider != null)
            {

                // Format the executable file name.
                // Build the output assembly path using the current directory
                // and <source>_cs.exe or <source>_vb.exe.

                String exeName = String.Format(@"{0}\{1}.exe",
                    System.Environment.CurrentDirectory,
                    sourceFile.Name.Replace(".", "_"));

                CompilerParameters cp = new CompilerParameters();

                // Generate an executable instead of 
                // a class library.
                cp.GenerateExecutable = false;

                // Specify the assembly file name to generate.
                cp.OutputAssembly = exeName;

                // Save the assembly as a physical file.
                cp.GenerateInMemory = true;

                // Set whether to treat all warnings as errors.
                cp.TreatWarningsAsErrors = false;

                cp.ReferencedAssemblies.AddRange(referencedAssemblies);

                // Invoke compilation of the source file.
                CompilerResults cr = provider.CompileAssemblyFromFile(cp,
                    sourceName);

                if (cr.Errors.Count > 0)
                {
                    // Display compilation errors.
                    Console.WriteLine("Errors building {0} into memory",
                        sourceName);
                    foreach (CompilerError ce in cr.Errors)
                    {
                        Console.WriteLine("  {0}", ce.ToString());
                        Console.WriteLine();
                    }
                }
                else
                {
                    // Display a successful compilation message.
                    Console.WriteLine("Source {0} built into memory successfully.",
                        sourceName);
                }

                // Return the results of the compilation.
                if (cr.Errors.Count > 0)
                {
                    compileOk = false;
                }
                else
                {
                    compileOk = true;
                    compiledAssembly = cr.CompiledAssembly;
                }
            }
            return compileOk;
        }
    }

    interface IWriter
    {
        void Write(TextWriter outWriter);
    }

    sealed class SequenceWriter : IWriter
    {
        public void Write(TextWriter outWriter)
        {
            foreach (IWriter w in writers)
            {
            }
        }
        public void AddWriter(IWriter w)
        {
            writers.Add(w);
        }
        IList<IWriter> writers;
    }

    sealed class StringWriter : IWriter
    {
        public void Write(TextWriter outWriter)
        {
            throw new Exception("The method or operation is not implemented.");
        }
   }

    abstract class AbstractWriter
    {
        protected void p(string value)
        {
            sb.Append(value);
        }
        protected void p(string format, params object[] arg)
        {
            sb.Append(string.Format(format, arg));
        }
        StringBuilder sb = new StringBuilder();
    }
}
