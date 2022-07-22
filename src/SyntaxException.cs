using System;

namespace Lang
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string msg)
            : base(msg)
        {
        }
    }
}