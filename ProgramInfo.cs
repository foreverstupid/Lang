using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Result of the syntax analysis, containing all needed
    /// tables for interpretation.
    /// </summary>
    public class ProgramInfo
    {
        public ProgramInfo(LinkedList<Rpn> program)
        {
            Program = program;
        }

        public LinkedList<Rpn> Program { get; }
    }
}