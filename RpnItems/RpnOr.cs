using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical OR operation.
    /// </summary>
    public class RpnOr : RpnOperation
    {
        public RpnOr(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}