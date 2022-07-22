using System;

namespace Lang.Exceptions
{
    public class LexicalAnalysisException : Exception
    {
        public LexicalAnalysisException(string message)
            : base(message)
        {
        }
    }
}