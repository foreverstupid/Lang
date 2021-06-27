using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical AND operation.
    /// </summary>
    public class RpnAnd : RpnBinaryOperation
    {
        public RpnAnd(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => new RpnInteger(left.GetBool() && right.GetBool());
    }
}