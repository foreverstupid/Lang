using System;

namespace Lang
{
    public class RpnCreationException : Exception
    {
        public RpnCreationException(string message)
            : base(message)
        {
        }
    }
}