using System;

namespace Lang.Exceptions
{
    public class InterpretationException : Exception
    {
        public InterpretationException(string msg)
            : base(msg)
        {
        }
    }
}