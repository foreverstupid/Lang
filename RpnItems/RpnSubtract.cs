namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents subtraction.
    /// </summary>
    public sealed class RpnSubtract : RpnBinaryOperation
    {
        public RpnSubtract(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AddSubPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(left.GetFloat() - right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() - right.GetInt()),
                RpnConst.Type.String =>
                    throw new InterpretationException("Cannot apply subtraction to string"),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };
    }
}