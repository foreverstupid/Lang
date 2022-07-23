using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical OR operation.
    /// </summary>
    public sealed class RpnOr : RpnLogicalOperation
    {
        public RpnOr(Token token, bool isReversed = false)
            : base(token, isReversed)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override bool GetBoolResult(RpnConst left, RpnConst right)
            => left.GetBool() || right.GetBool();
    }
}