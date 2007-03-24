using System;

namespace EventSystem
{
    internal class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error)
        {
        }
    }
}