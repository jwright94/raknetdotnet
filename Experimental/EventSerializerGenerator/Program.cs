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

    [AttributeUsage(AttributeTargets.Class)]
    public class EventAttribute : Attribute
    {
        public bool IsBroadcast = false;
        public bool IsTwoWay = false;
        public bool RunOnServer = false;
        public bool PerformBeforeConnectOnClient = false;
    }

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
            List<string> referencedAssemblies = new List<string>();

            if(commaSeparatedReferencedAssemblies != null)
            {
                referencedAssemblies = new List<string>(commaSeparatedReferencedAssemblies.Split(','));
            }
            if (!referencedAssemblies.Contains("EventSerializerGenerator.exe"))
            {
                referencedAssemblies.Add("EventSerializerGenerator.exe");
            }

            if (!CompileExecutable(eventTemplatePath, referencedAssemblies.ToArray(), out templateAssembly))
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
        void Write(ICodeWriter o);
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
        public abstract void Write(ICodeWriter o);
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
        public override void Write(ICodeWriter o)
        {
            o.WriteLine("using System;");
            o.WriteLine("using System.Collections.Generic;");
            o.WriteLine("using System.Text;");
            o.WriteLine("using RakNetDotNet;");
            o.WriteLine("using EventSystem;");
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
        }
        Assembly templateAssembly;
    }
    sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string _namespace, IEnumerable<Type> typesInNamespace)  // TODO: I forgot how to use simbol.
        {
            this._namespace = _namespace;
            IList<EventInfo> eventInfos = new List<EventInfo>();
            foreach (Type type in typesInNamespace)
            {
                EventInfo ei = new EventInfo();
                ei.Type = type;
                ei.EventId = eventInfos.Count;
                eventInfos.Add(ei);
                AddChildGenerator(new ClassGenerator(ei.Type, ei.EventId));
            }
            AddChildGenerator(new EventFactoryGenerator(FactoryName, eventInfos));
        }
        public override void Write(ICodeWriter o)
        {
            o.BeginBlock("namespace {0} {{", _namespace);
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
            o.EndBlock("}");
        }
        string FactoryName
        {
            get
            {
                string factoryName;
                int pos = _namespace.LastIndexOf("Events");
                if (0 < pos)
                {
                    factoryName = _namespace.Substring(0, pos);
                }
                else
                {
                    factoryName = _namespace;
                }
                factoryName += "EventFactory";
                return factoryName;
            }
        }
        string _namespace;
    }
    sealed class ClassGenerator : IGenerator
    {
        public ClassGenerator(Type type, int eventId)
        {
            this.type = type;
            this.eventId = eventId;
        }
        public void AddChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void RemoveChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Write(ICodeWriter o)
        {
            o.BeginBlock("public partial class {0} : IEvent {{", type.Name);
            WriteCtorWithId(o);
            WriteCtorWithStream(o);
            WriteSetData(o);
            WriteGetStream(o);
            WriteId(o);
            WriteOriginPlayer(o);
            o.EndBlock("}");
        }
        void WriteCtorWithId(ICodeWriter o)
        {
            o.BeginBlock("public {0}() {{", type.Name);
            o.WriteLine("id = {0};", eventId);
            o.EndBlock("}");
        }
        void WriteCtorWithStream(ICodeWriter o)
        {
            o.BeginBlock("public {0}(BitStream source) {{", type.Name);
            WriteStreamReadStatement(o, "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteStreamReadStatement(o, field.Name);
            }
            o.EndBlock("}");
        }
        void WriteSetData(ICodeWriter o)
        {
            StringBuilder arg = new StringBuilder();
            FieldInfo[] fields = GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (0 < i) arg.Append(", ");
                arg.Append(fields[i].FieldType.ToString());
                arg.Append(" ");
                arg.Append(fields[i].Name);
            }
            o.BeginBlock("public void SetData({0}) {{", arg.ToString());
            foreach (FieldInfo field in fields)
            {
                o.WriteLine("this.{0} = {0};", field.Name);
            }
            o.EndBlock("}");
        }
        void WriteStreamReadStatement(ICodeWriter o, string fieldName)
        {
            o.WriteLine("if (!source.Read(out {0})) {{ throw new NetworkException(\"Deserialization is failed.\"); }}", fieldName);
        }
        void WriteGetStream(ICodeWriter o)
        {
            o.BeginBlock("public void BitStream Stream {");
            o.BeginBlock("get {");
            o.WriteLine("BitStream eventStream = new BitStream();");
            WriteStreamWriteStatement(o, "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteStreamWriteStatement(o, field.Name);
            }
            o.WriteLine("return eventStream;");
            o.EndBlock("}");
            o.EndBlock("}");
        }
        void WriteStreamWriteStatement(ICodeWriter o, string fieldName)
        {
            o.WriteLine("eventStream.Write({0});", fieldName);
        }
        void WriteId(ICodeWriter o)
        {
            o.BeginBlock("public int Id {");
            o.WriteLine("get { return id; }");
            o.WriteLine("protected set { id = value; }");
            o.EndBlock("}");
            o.WriteLine("int id;");
        }
        void WriteOriginPlayer(ICodeWriter o)
        {
            o.BeginBlock("public SystemAddress OriginPlayer {");
            o.WriteLine("get { return originPlayer; }");
            o.WriteLine("set { originPlayer = value; }");
            o.EndBlock("}");
            o.WriteLine("SystemAddress originPlayer = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;");
        }
        void WriteBehaviorFlags(ICodeWriter o)
        {
            //type.GetCustomAttributes(typeof(EventAttribute), true);
        }
        FieldInfo[] GetFields()
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        Type type;
        int eventId;
    }
    sealed class EventFactoryGenerator : IGenerator
    {
        public EventFactoryGenerator(string factoryName, IList<EventInfo> eventInfos)
        {
            this.factoryName = factoryName;
            this.eventInfos = eventInfos;
        }
        public void AddChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void RemoveChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Write(ICodeWriter o)
        {
            o.WriteLine("[Singleton]");
            o.BeginBlock("sealed class {0} : IEventFactory {{", factoryName);
            WriteRecreateEvent(o);
            o.EndBlock("}");
        }
        void WriteRecreateEvent(ICodeWriter o)
        {
            o.BeginBlock("public IEvent RecreateEvent(BitStream source) {");
            o.WriteLine("Debug.Assert(source != null);");
            o.WriteLine("IEvent _event;");
            o.WriteLine("int id;");
            o.WriteLine("if(!source.Read(out id)) throw new NetworkException(\"Deserialization is failed.\");");
            o.WriteLine("source.ResetReadPointer();");
            o.BeginBlock("switch (id) {");
            foreach (EventInfo ei in eventInfos)
            {
                o.BeginBlock("case {0}:", ei.EventId);
                o.WriteLine("_event = new {0}(source);", ei.Type.Name);
                o.WriteLine("break;");
                o.EndBlock("");
            }
            o.BeginBlock("default:");
            o.WriteLine("throw new NetworkException(string.Format(\"Event id {{0}} not recognized by {0}.RecreateEvent()!\", id));", factoryName);
            o.EndBlock("");
            o.EndBlock("}");
            o.WriteLine("return _event;");
            o.EndBlock("}");
        }
        string factoryName;
        IList<EventInfo> eventInfos;
    }
    interface ICodeWriter
    {
        void BeginBlock(string format, params object[] arg);
        void EndBlock(string format, params object[] arg);
        void WriteLine(string format, params object[] arg);
    }
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
    class EventInfo
    {
        public Type Type;
        public int EventId;
    }
    #endregion

}
