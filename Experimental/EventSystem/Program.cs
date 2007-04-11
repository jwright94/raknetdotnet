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
            IServerHost host = new ServerHost();
            host.Run(args);
        }
    }
}