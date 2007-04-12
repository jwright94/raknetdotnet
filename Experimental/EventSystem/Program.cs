using CommandLine;

namespace EventSystem
{
    internal class AppArguments
    {
        [DefaultArgument(ArgumentType.Required, HelpText = "Configuration xml filename.")] public string ConfigurationFilename;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO - We can remove IServerHost and ServerHost.
            IServerHost host = new ServerHost();
            host.Run(args);
        }
    }
}