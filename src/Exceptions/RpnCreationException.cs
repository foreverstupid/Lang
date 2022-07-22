using System;

namespace Lang.Exceptions
{
    public class RpnCreationException : Exception
    {
        public RpnCreationException(string message)
            : base(message)
        {
        }
    }
}