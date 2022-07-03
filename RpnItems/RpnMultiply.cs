using System.Linq;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents multiplication.
    /// </summary>
    public sealed class RpnMultiply : RpnBinaryOperation
    {
        public RpnMultiply(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.MulDivPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(left.GetFloat() * right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() * right.GetInt()),
                RpnConst.Type.String =>
                    right.ValueType == RpnConst.Type.Integer && right.GetInt() >= 0
                    ? new RpnString(string.Join("", Enumerable.Repeat(left.GetString(), right.GetInt())))
                    : throw new InterpretationException("Cannot multiply string"),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };
    }
}