using System;

namespace EventSystem
{
    // TODO - Add more exception.
    internal class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error)
        {
        }
    }
}