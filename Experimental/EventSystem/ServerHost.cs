using System;
using System.Runtime.InteropServices;
using System.Threading;
using Castle.Core.Logging;
using CommandLine;

namespace EventSystem
{
    public sealed class ServerHost
    {
        public static void Run(string[] args)
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
        internal static extern int _kbhit(); // I do not want to use this.
    }
}