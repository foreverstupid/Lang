namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents equality comparision.
    /// </summary>
    public class RpnEqual : RpnBinaryOperation
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
                RpnConst.Type.Float => new RpnInteger(left.GetFloat() == right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() == right.GetInt()),
                RpnConst.Type.String => new RpnInteger(left.GetString() == right.GetString()),
                RpnConst.Type.BuiltIn =>
                    right.ValueType == RpnConst.Type.BuiltIn
                    ? new RpnInteger(left.GetString() == right.GetString())
                    : new RpnInteger(false),
                RpnConst.Type.Func =>
                    right.ValueType == RpnConst.Type.Func
                    ? new RpnInteger(left.GetString() == right.GetString())
                    : new RpnInteger(false),
                RpnConst.Type.Variable =>
                    right.ValueType == RpnConst.Type.Variable
                    ? new RpnInteger(left.GetString() == right.GetString())
                    : new RpnInteger(false),
                _ => new RpnInteger(false)
            };
    }
}