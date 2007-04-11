using System;

namespace EventSystem
{
    // TODO - Write comments. Where does this exception throw?
    internal class NetworkException : ApplicationException
    {
        public NetworkException()
        {}
        public NetworkException(string message)
            : base(message)
        {
        }
    }

    internal class DeserializationException : NetworkException
    {
        public DeserializationException() : base("Deserialization is failed.")
        {
            
        }
        public DeserializationException(string message) : base(message)
        {
            
        }
    }

    internal class UnknownEventIdException : NetworkException
    {
        public UnknownEventIdException()
        {
        }

        public UnknownEventIdException(string message) : base(message)
        {
            
        }
        public UnknownEventIdException(int eventId, string thrower)
            : base(string.Format("Event id {0} not recognized by {1}!", eventId, thrower))
        {
            
        }
    }
}