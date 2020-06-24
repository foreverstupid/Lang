using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents less comparision operation.
    /// </summary>
    public class RpnLess : RpnBinaryOperation
    {
        public RpnLess(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => new RpnInteger(left.GetFloat() < right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() < right.GetInt()),
                RpnConst.Type.String => new RpnInteger(IsLess(left.GetString(), right.GetString())),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };

        private bool IsLess(string s1, string s2) => s1.CompareTo(s2) < 0;
    }
}