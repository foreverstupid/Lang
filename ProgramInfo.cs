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
        public List<Rpn> Rpns { get; set; }
    }
}