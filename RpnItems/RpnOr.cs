namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical OR operation.
    /// </summary>
    public sealed class RpnOr : RpnBinaryOperation
    {
        public RpnOr(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.LogicalOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => RpnConst.Bool(left.GetBool() || right.GetBool());
    }
}