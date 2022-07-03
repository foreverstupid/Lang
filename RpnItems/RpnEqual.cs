namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents equality comparision.
    /// </summary>
    public sealed class RpnEqual : RpnBinaryOperation
    {
        public RpnEqual(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => RpnConst.Bool(left.GetFloat() == right.GetFloat()),
                RpnConst.Type.Integer => RpnConst.Bool(left.GetInt() == right.GetInt()),
                RpnConst.Type.String => RpnConst.Bool(left.GetString() == right.GetString()),
                RpnConst.Type.BuiltIn =>
                    right.ValueType == RpnConst.Type.BuiltIn
                    ? RpnConst.Bool(left.GetString() == right.GetString())
                    : RpnConst.False,
                RpnConst.Type.Func =>
                    right.ValueType == RpnConst.Type.Func
                    ? RpnConst.Bool(left.GetString() == right.GetString())
                    : RpnConst.False,
                RpnConst.Type.Variable =>
                    right.ValueType == RpnConst.Type.Variable
                    ? RpnConst.Bool(left.GetString() == right.GetString())
                    : RpnConst.False,
                _ => RpnConst.False
            };
    }
}