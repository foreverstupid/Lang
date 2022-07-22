using System.Linq;
using Lang.Exceptions;
using Lang.Pipeline;

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
                    right.ValueType == RpnConst.Type.Integer
                    ? new RpnString(ShiftStringChars(left.GetString(), right.GetInt()))
                    : throw new InterpretationException(
                        "Cannot apply subtraction to string and not integer number"),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}")
            };

        private static string ShiftStringChars(string str, int shift)
        {
            var result = new string(str.Select(ch => (char)(ch - shift)).ToArray());
            return result;
        }
    }
}