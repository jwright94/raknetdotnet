using System;
using System.Collections.Generic;
using System.Text;

namespace RakNetDotNet
{
    using System.Reflection;

    public static class RakPeerHelper
    {
        public static void RegisterClassMemberRPC<Interface>(RakPeerInterface rakPeer)
        {
            MethodInfo[] methods = typeof(Interface).GetMethods();
            foreach (MethodInfo method in methods)
            {
                rakPeer.RegisterClassMemberRPC(method);
            }
        }
    }
}
