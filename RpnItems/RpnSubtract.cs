using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents subtraction.
    /// </summary>
    public class RpnSubtract : RpnOperation
    {
        public RpnSubtract(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AddSubPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}