using System;
using System.Collections.Generic;
using System.Text;

namespace CommandConsoleServer
{
    using RakNetDotNet;

    class Program
    {
        static void Main(string[] args)
        {
            TelnetTransport tt = new TelnetTransport();
            RakNetTransport rt = new RakNetTransport();
            TestCommandServer(tt, 23);
        }

        static void TestCommandServer(TransportInterface ti, ushort port)
        {
            ConsoleServer consoleServer = new ConsoleServer();
            RakNetCommandParser rcp = new RakNetCommandParser();
            LogCommandParser lcp = new LogCommandParser();
            uint lastlog = 0;
            RakPeerInterface rakPeer = RakNetworkFactory.GetRakPeerInterface();
            IntPtr testChannel = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi("TestChannel");  // you must call FreeHGlobal

            Console.WriteLine("Command server started on port {0}.", port);
            consoleServer.AddCommandParser(rcp);
            consoleServer.AddCommandParser(lcp);
            consoleServer.SetTransportProvider(ti, port);
            rcp.SetRakPeerInterface(rakPeer);
            lcp.AddChannel(testChannel);
            while (true)
            {
                consoleServer.Update();

                if (RakNetBindings.GetTime() > lastlog + 4000)
                {
                    lcp.WriteLog(testChannel, "Test of logger");
                    lastlog = RakNetBindings.GetTime();
                }

                System.Threading.Thread.Sleep(30);
            }
        }
    }
}
