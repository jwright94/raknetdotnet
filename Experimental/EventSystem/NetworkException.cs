using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    class NetworkException : ApplicationException
    {
        public NetworkException(string error) : base(error) { }
    }
}
