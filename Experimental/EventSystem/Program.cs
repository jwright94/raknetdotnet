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

    public interface IServerHost
    {
        void Main(string[] args);
    }

    public sealed class ServerHost : IServerHost
    {
        public void Main(string[] args)
        {
            AppArguments parsedArgs = new AppArguments();
            if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                return;
            }

            LightweightContainer.Configure(parsedArgs.ConfigurationFilename);
            ILogger logger = LightweightContainer.LogFactory.Create("Global");
            IServer server = LightweightContainer.Resolve<IServer>();
            server.Startup();
            logger.Info("Server is started.");
            while (true)
            {
                if (_kbhit() != 0)
                {
                    char ch = Console.ReadKey(true).KeyChar;
                    if (ch == 'q' || ch == 'Q')
                    {
                        break;
                    }
                }
                server.Update();
                Thread.Sleep(server.SleepTimer);
            }
            server.Shutdown();
            logger.Info("Server is shutdowned.");
        }

        [DllImport("crtdll.dll")]
        public static extern int _kbhit(); // I do not want to use this.
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            IServerHost serverHost = new ServerHost();
            serverHost.Main(args);
        }
    }
}