using System.Collections.Generic;

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
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };
    }
}