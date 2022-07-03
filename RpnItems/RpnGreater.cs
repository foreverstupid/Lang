namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents greater comparision operation.
    /// </summary>
    public sealed class RpnGreater : RpnBinaryOperation
    {
        public RpnGreater(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => RpnConst.Bool(left.GetFloat() > right.GetFloat()),
                RpnConst.Type.Integer => RpnConst.Bool(left.GetInt() > right.GetInt()),
                RpnConst.Type.String => RpnConst.Bool(IsGreater(left.GetString(), right.GetString())),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };

        private bool IsGreater(string s1, string s2) => s1.CompareTo(s2) > 0;
    }
}