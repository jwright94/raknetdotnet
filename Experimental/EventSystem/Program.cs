using System;
using System.Runtime.InteropServices;
using System.Threading;
using Castle.Core.Logging;
using CommandLine;

namespace EventSystem
{
    internal class AppArguments
    {
        [DefaultArgument(ArgumentType.Required, HelpText = "Configuration xml filename.")]
        public string ConfigurationFilename;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerHost.Run(args);
        }
    }
}