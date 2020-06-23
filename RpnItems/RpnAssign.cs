using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment some value to the variable.
    /// </summary>
    public class RpnAssign : RpnOperation
    {
        public RpnAssign(Token token)
            : base(token)
        { 
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AssignmentPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}