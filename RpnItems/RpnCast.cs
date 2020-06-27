namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that performs casting of the left operand to the type of the
    /// right one.
    /// </summary>
    public class RpnCast : RpnBinaryOperation
    {
        public RpnCast(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.CastPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => right.ValueType switch
            {
                RpnConst.Type.Integer => new RpnInteger(left.GetInt()),
                RpnConst.Type.Float => new RpnFloat(left.GetFloat()),
                RpnConst.Type.String => new RpnString(left.GetString()),
                var type => throw new InterpretationException(
                    $"Cannot cast to the type '{type}'"
                )
            };
    }
}