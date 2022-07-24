using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents addition.
    /// </summary>
    public sealed class RpnAdd : RpnBinaryOperation
    {
        public RpnAdd(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AddSubPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(left.GetFloat() + right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() + right.GetInt()),
                RpnConst.Type.String => new RpnString(left.GetString() + right.GetString()),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}")
            };
    }
}