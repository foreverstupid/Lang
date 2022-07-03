namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents negation.
    /// </summary>
    public sealed class RpnNegate : RpnUnaryOperation
    {
        public RpnNegate(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.UnarOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
            => operand.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(-operand.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(-operand.GetInt()),
                RpnConst.Type.String =>
                    throw new InterpretationException("String cannot be negated"),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the operand: {type}"
                    )
            };
    }
}