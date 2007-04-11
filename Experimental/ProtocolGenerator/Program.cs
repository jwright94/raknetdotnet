using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using CommandLine;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NUnit.Framework;
using ProtocolGenerator.Generators;

namespace ProtocolGenerator
{
    // CodeDom is complicated. I use a simpler method. It is Console.WriteLine.
    internal class Program
    {
        #region Static Functions

        private static int Main(string[] args)
        {
            AppArguments parsedArgs = new AppArguments();
            if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                return 1;
            }

            List<string> referencedAssemblies = new List<string>(parsedArgs.ReferencedAssemblies);
            if (
                !referencedAssemblies.Exists(
                     delegate(string asm) { return asm.Contains("ProtocolGenerator.exe"); }))
            {
                referencedAssemblies.Add("ProtocolGenerator.exe");
            }

            Assembly templateAssembly;

            if (!CompileLibraryInMemory(parsedArgs.EventTemplatePath, referencedAssemblies.ToArray(), out templateAssembly))
            {
                Console.WriteLine("ERROR: TemplateFile compile error.");
                return 2;
            }

            IGenerator rootGenerator;
            try
            {
                // TODO - Ctor throws exception. Is it OK?
                rootGenerator = new RootGenerator(templateAssembly.GetTypes());
            }
            catch (SyntaxErrorException e)
            {
                Console.WriteLine(e);
                return 3;
            }

            string generatedFilePath = GetGeneratedFilePath(parsedArgs.EventTemplatePath);
            using (StreamWriter sw = File.CreateText(generatedFilePath))
            {
                rootGenerator.Write(new CodeWriter(sw));
            }
            return 0;
        }

        private static bool CompileLibraryInMemory(String sourceName, string[] referencedAssemblies,
                                                   out Assembly compiledAssembly)
        {
            FileInfo sourceFile = new FileInfo(sourceName);
            CodeDomProvider provider;
            bool compileOk;
            compiledAssembly = null;

            if (!sourceFile.Exists)
            {
                Console.WriteLine("{0} file not found.", sourceName);
                return false;
            }

            // Select the code provider based on the input file extension.
            if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
            {
                provider = new CSharpCodeProvider();
            }
            else if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".VB")
            {
                provider = new VBCodeProvider();
            }
            else
            {
                Console.WriteLine("Source file must have a .cs or .vb extension");
                return false;
            }

            // Format the executable file name.
            // Build the output assembly path using the current directory
            // and <source>_cs.exe or <source>_vb.exe.

            String exeName = String.Format(@"{0}\{1}.exe",
                                           Environment.CurrentDirectory,
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
                    Console.WriteLine("  {0}", ce);
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

            return compileOk;
        }

        #endregion

        #region Private Utility Functions

        private static string GetGeneratedFilePath(string eventTemplatePath)
        {
            string dir = Path.GetDirectoryName(eventTemplatePath);
            string file = Path.GetFileNameWithoutExtension(eventTemplatePath);
            string ext = Path.GetExtension(eventTemplatePath);
            return dir + "\\" + file + ".generated" + ext;
        }

        #endregion
    }

    internal static class NamingHelper
    {
        public static string GetPrefix(string name, string conventionName)
        {
            string prefix;
            int pos = name.LastIndexOf(conventionName);
            if (0 < pos)
            {
                prefix = name.Substring(0, pos);
            }
            else
            {
                prefix = name;
            }
            return prefix;
        }
    }

    [TestFixture]
    public sealed class ProgramTestCase
    {
        [Test]
        public void GeneratedFileName()
        {
            string eventTemplatePath = @"c:\home\white space\sampleevents.cs";
            string generatedFilePath =
                (string)PrivateAccessor.ExecuteStaticMethod(typeof (Program), "GetGeneratedFilePath", new object[] {eventTemplatePath});
            Assert.AreEqual(@"c:\home\white space\sampleevents.generated.cs", generatedFilePath);
        }
    }
}