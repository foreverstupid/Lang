namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical NOT operation.
    /// </summary>
    public sealed class RpnNot : RpnUnaryOperation
    {
        public RpnNot()
            : base(null)
        {
        }

        public RpnNot(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
            => RpnConst.Bool(!operand.GetBool());
    }
}