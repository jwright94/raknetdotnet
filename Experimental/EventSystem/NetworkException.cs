using System;

namespace EventSystem
{
    // TODO - Add more exception.
    // TODO - Write comments. Where does this exception throw?
    internal class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error)
        {
        }
    }
}