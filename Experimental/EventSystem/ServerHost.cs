using System;
using System.Collections.Generic;
using System.Threading;
using Castle.Core.Logging;
using CommandLine;

namespace EventSystem
{
    public interface IServerHost
    {
        void Run(string[] args);
    }

    public sealed class ServerHost : IServerHost
    {
        public void Run(string[] args)
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
            BeginReadThread();
            while (true)
            {
                lock (commandLineQueue)
                {
                    bool doQuit = false;
                    while (0 < commandLineQueue.Count)
                    {
                        string command = commandLineQueue.Dequeue();
                        if (command == "quit")
                        {
                            doQuit = true;
                        }

                        if (command == "cc")
                        {
                            if (server is Client)
                            {
                                ((Client)server).ChangeColor();
                            }
                        }
                    }
                    if (doQuit)
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

        private void BeginReadThread()
        {
            shouldStop = false;
            readDelegate = new AsyncReadCommandDelegate(AsyncReadCommand);
            ar = readDelegate.BeginInvoke(null, null);
        }

        /// <summary>
        /// 標準入力からコマンドを読み込みcommandQueueに入れる。
        /// </summary>
        private void AsyncReadCommand()
        {
            while (!shouldStop)
            {
                string command = Console.ReadLine();
                lock (commandLineQueue)
                {
                    commandLineQueue.Enqueue(command);
                }
                Thread.Sleep(0);
            }
        }

        private AsyncReadCommandDelegate readDelegate;
        private IAsyncResult ar;
        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool shouldStop;

        private delegate void AsyncReadCommandDelegate();

        private Queue<string> commandLineQueue = new Queue<string>();
    }
}