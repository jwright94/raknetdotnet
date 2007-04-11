using System;
using System.Collections.Generic;
using System.Text;
using EventSystem;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IServerHost serverHost = new ServerHost();
            serverHost.Main(args);
        }
    }
}
