using System;

namespace Lang
{
    public class LexicalAnalysisException : Exception
    {
        public LexicalAnalysisException(string message)
            : base(message)
        {
        }
    }
}