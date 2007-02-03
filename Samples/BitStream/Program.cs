using System;
using System.Collections.Generic;
using System.Text;

namespace BitStream
{
    using RakNetDotNet;

    class Program
    {
        static bool quit;

        struct EmploymentStruct
        {
            int salary;
            byte yearsEmployed;
        }

        static void clientRPC(RPCParameters rpcParameters)
        {
            BitStream b = new BitStream(rpcParameters, false);
        }

        static void Main(string[] args)
        {
            RakPeerInterface rakClient = RakNetworkFactory.GetRakPeerInterface();
	        RakPeerInterface rakServer = RakNetworkFactory.GetRakPeerInterface();


        }
    }
}
