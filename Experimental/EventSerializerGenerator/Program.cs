using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Globalization;
using Microsoft.CSharp;
using NUnit.Framework;
using CommandLine;

namespace EventSerializerGenerator
{
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

        #region Private Utility Functions
        int Run(string[] cmdLine)
        {
            AppArguments parsedArgs = new AppArguments();
            if (!Parser.ParseArgumentsWithUsage(cmdLine, parsedArgs))
            {
                return 1;
            }

            List<string> referencedAssemblies = new List<string>(parsedArgs.ReferencedAssemblies);
            if (!referencedAssemblies.Exists(delegate(string asm)
            {
                return asm.Contains("EventSerializerGenerator.exe");
            }))
            {
                referencedAssemblies.Add("EventSerializerGenerator.exe");
            }

            Assembly templateAssembly = null;

            if (!CompileExecutable(parsedArgs.EventTemplatePath, referencedAssemblies.ToArray(), out templateAssembly))
            {
                Console.WriteLine("ERROR: TemplateFile compile error.");
                return 2;
            }

            IGenerator rootWriter = new RootGenerator(templateAssembly.GetTypes());
            string generatedFilePath = GetGeneratedFilePath(parsedArgs.EventTemplatePath);
            using (StreamWriter sw = File.CreateText(generatedFilePath))
            {
                rootWriter.Write(new CodeWriter(sw));
            }
            return 0;
        }
        string GetGeneratedFilePath(string eventTemplatePath)
        {
            string dir = Path.GetDirectoryName(eventTemplatePath);
            string file = Path.GetFileNameWithoutExtension(eventTemplatePath);
            string ext = Path.GetExtension(eventTemplatePath);
            return dir + "\\" + file + ".generated" + ext;
        }
        #endregion
    }

    [TestFixture]
    public sealed class ProgramTestCase
    {
        [Test]
        public void GeneratedFileName()
        {
            string eventTemplatePath = @"c:\home\white space\sampleevents.cs";
            string generatedFilePath = (string)PrivateAccessor.ExecuteMethod(new Program(), "GetGeneratedFilePath", eventTemplatePath);
            Assert.AreEqual(@"c:\home\white space\sampleevents.generated.cs", generatedFilePath);
        }
    }
}
