using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical AND operation.
    /// </summary>
    public sealed class RpnAnd : RpnBinaryOperation
    {
        public RpnAnd(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => RpnConst.Bool(left.GetBool() && right.GetBool());
    }
}