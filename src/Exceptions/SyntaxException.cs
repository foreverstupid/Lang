using System;

namespace Lang.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string msg)
            : base(msg)
        {
        }
    }
}