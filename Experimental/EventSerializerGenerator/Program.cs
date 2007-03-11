using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    using System.Reflection;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Globalization;
    using Microsoft.CSharp;
    using RJH.CommandLineHelper;

    // CodeDom is complicated. I use a simpler method. It is Console.WriteLine.
    class Program
    {
        #region Static Functions
        static int Main(string[] args)
        {
            Program prog = new Program();
            return prog.Run(args);
        }
        static bool CompileExecutable(String sourceName, string[] referencedAssemblies, out Assembly compiledAssembly)
        {
            FileInfo sourceFile = new FileInfo(sourceName);
            CodeDomProvider provider = null;
            bool compileOk = false;
            compiledAssembly = null;

            if (!sourceFile.Exists)
            {
                Console.WriteLine("{0} file not found.", sourceName);
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
        #endregion

        #region Command Line SWitches
        [CommandLineSwitch("help", "Show help.")]
        [CommandLineAlias(@"\?")]
        public bool ShowHelp
        {
            set { showHelp = value; }
            get { return showHelp; }
        }
        [CommandLineSwitch("refasm", "A comma separated list of referenced assemblies.")]
        public string CommaSeparatedReferencedAssemblies
        {
            set { commaSeparatedReferencedAssemblies = value; }
            get { return commaSeparatedReferencedAssemblies; }
        }
        #endregion

        #region Private Utility Functions
        int Run(string[] cmdLine)
        {
            // TODO: Automatic Command Line Parser doesn't treat double-quoted string. I parse cmdLine manually now.
            //Parser parser = new Parser(System.Environment.CommandLine, this);

            //parser.Parse();

            //if (showHelp)
            //{
            //    Console.WriteLine("EventSerializerGenerator TemplateFile [Options] ...\n");
            //    Console.WriteLine("---- Options ----");
            //    foreach (Parser.SwitchInfo s in parser.Switches)
            //    {
            //        Console.WriteLine("{0} : {1}", s.Name, s.Description);
            //    }
            //    return 0;
            //}

            //if (parser.Parameters.Length <= 0)
            //{
            //    Console.WriteLine("ERROR: You must pass TemplateFile.");
            //    return 1;
            //}

            //string eventTemplatePath = parser.Parameters[0];

            string eventTemplatePath = null;
            string mode = null;

            for (int i = 0; i < cmdLine.Length; i++)
            {
                // Change mode.
                if (cmdLine[i].Contains("help"))
                {
                    // Ignore this.
                    continue;
                }
                else if (cmdLine[i].Contains("refasm"))
                {
                    mode = "refasm";
                    continue;
                }

                // Store parameters.
                string trimmedString = cmdLine[i].Trim(' ', '"');
                switch (mode)
                {
                    case "refasm":
                        commaSeparatedReferencedAssemblies = trimmedString;
                        break;
                    default:
                        if (eventTemplatePath == null)
                            eventTemplatePath = trimmedString;
                        break;
                }
            }

            Assembly templateAssembly = null;
            string[] referencedAssemblies = new string[0];

            if(commaSeparatedReferencedAssemblies != null)
            {
                referencedAssemblies = commaSeparatedReferencedAssemblies.Split(',');
            }

            if (!CompileExecutable(eventTemplatePath, referencedAssemblies, out templateAssembly))
            {
                Console.WriteLine("ERROR: TemplateFile compile error.");
                return 2;
            }

            IGenerator rootWriter = new RootGenerator(templateAssembly);
            rootWriter.Write(new CodeWriter(Console.Out));

            return 0;
        }
        #endregion

        #region Private Variables
        bool showHelp;
        string commaSeparatedReferencedAssemblies;
        #endregion
    }
    #region Writers
    // I use the composit pattern.
    interface IGenerator
    {
        void AddChildGenerator(IGenerator generator);
        void RemoveChildGenerator(IGenerator generator);
        void Write(ICodeWriter outWriter);
    }
    abstract class AbstractGenerator : IGenerator
    {
        public void AddChildGenerator(IGenerator generator)
        {
            generators.Add(generator);
        }
        public void RemoveChildGenerator(IGenerator generator)
        {
            generators.Remove(generator);
        }
        public abstract void Write(ICodeWriter outWriter);
        protected ICollection<IGenerator> generators = new List<IGenerator>();
    }
    sealed class RootGenerator : AbstractGenerator
    {
        public RootGenerator(Assembly templateAssembly)
        {
            this.templateAssembly = templateAssembly;
            Type[] allTypes = this.templateAssembly.GetTypes();
            IDictionary<string, ICollection<Type>> namespaceTypesHash = new Dictionary<string, ICollection<Type>>();
            foreach (Type type in allTypes)
            {
                ICollection<Type> typesInNamespace;
                if (!namespaceTypesHash.TryGetValue(type.Namespace, out typesInNamespace))
                {
                    typesInNamespace = new List<Type>();
                    namespaceTypesHash[type.Namespace] = typesInNamespace;
                }
                typesInNamespace.Add(type);
            }
            foreach(KeyValuePair<string, ICollection<Type>> namespaceTypes in namespaceTypesHash)
            {
                AddChildGenerator(new NamespaceGenerator(namespaceTypes.Key, namespaceTypes.Value));
            }
        }
        public override void Write(ICodeWriter outWriter)
        {
            outWriter.WriteLine("using System;");
            outWriter.WriteLine("using System.Collections.Generic;");
            outWriter.WriteLine("using System.Text;");
            outWriter.WriteLine("using RakNetDotNet;");
            foreach (IGenerator generator in generators)
            {
                generator.Write(outWriter);
            }
        }
        Assembly templateAssembly;
    }
    sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string _namespace, IEnumerable<Type> typesInNamespace)  // TODO: I forgot how to use simbol.
        {
            this._namespace = _namespace;
            foreach (Type type in typesInNamespace)
            {
                AddChildGenerator(new ClassGenerator(type));
            }
        }
        public override void Write(ICodeWriter outWriter)
        {
            outWriter.WriteLine("namespace {0} {{", _namespace);
            outWriter.Indent();
            foreach (IGenerator generator in generators)
            {
                generator.Write(outWriter);
            }
            outWriter.Deindent();
            outWriter.WriteLine("}");
        }
        string _namespace;
    }
    sealed class ClassGenerator : IGenerator
    {
        public ClassGenerator(Type type)
        {
            this.type = type;
        }
        public void AddChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void RemoveChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Write(ICodeWriter outWriter)
        {
            outWriter.WriteLine("public partial class {0} {{", type.Name);
            outWriter.Indent();
            WriteDeserializer(outWriter);
            outWriter.Deindent();
            outWriter.WriteLine("}");
        }
        void WriteDeserializer(ICodeWriter outWriter)
        {
            outWriter.WriteLine("public {0}(BitStream source) {{", type.Name);
            outWriter.Indent();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                outWriter.WriteLine("source.Read(out {0});", field.Name);
            }
            outWriter.Deindent();
            outWriter.WriteLine("}");
        }
        void WriteSerializer(ICodeWriter outWriter)
        {
            outWriter.WriteLine("");
        }
        Type type;
    }
    interface ICodeWriter
    {
        void Indent();
        void Deindent();
        void WriteLine(string value);
        void WriteLine(string format, params object[] arg);
    }
    sealed class CodeWriter : ICodeWriter
    {
        public CodeWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }
        public void Indent()
        {
            levelOfIndent++;
        }
        public void Deindent()
        {
            levelOfIndent = Math.Max(0, levelOfIndent - 1);
        }
        public void WriteLine(string value)
        {
            InsertTabs();
            textWriter.WriteLine(value);
        }
        public void WriteLine(string format, params object[] arg)
        {
            WriteLine(string.Format(format, arg));
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
    #endregion

}
