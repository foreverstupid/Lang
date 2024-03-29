using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents modulo division.
    /// </summary>
    public sealed class RpnMod : RpnBinaryOperation
    {
        public RpnMod(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.MulDivPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(left.GetFloat() % right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() % right.GetInt()),
                RpnConst.Type.String =>
                    throw new InterpretationException("String cannot be divided by modulo"),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };
    }
}