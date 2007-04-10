using System;

namespace ProtocolGenerator
{
    internal sealed class SyntaxErrorException : ApplicationException
    {
        public SyntaxErrorException(string error) : base(error)
        {
        }
    }
}