using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical AND operation.
    /// </summary>
    public sealed class RpnAnd : RpnLogicalOperation
    {
        public RpnAnd(Token token, bool isReversed = false)
            : base(token, isReversed)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override bool GetBoolResult(RpnConst left, RpnConst right)
            => left.GetBool() && right.GetBool();
    }
}