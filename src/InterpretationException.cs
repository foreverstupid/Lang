using System;

namespace Lang
{
    public class InterpretationException : Exception
    {
        public InterpretationException(string msg)
            : base(msg)
        {
        }
    }
}