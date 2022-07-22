using System.Linq;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents shift operation.
    /// </summary>
    public abstract class RpnShift : RpnBinaryOperation
    {
        public RpnShift(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected sealed override int Priority => RpnOperation.ShiftPriority;

        /// <inheritdoc/>
        protected sealed override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (right.ValueType != RpnConst.Type.Integer)
            {
                throw new InterpretationException(
                    "The right operand of the shift operation should be an integer");
            }

            if (left.ValueType != RpnConst.Type.Integer &&
                left.ValueType != RpnConst.Type.String)
            {
                throw new InterpretationException(
                    "The shift operation is only allowed for integers or strings");
            }

            var shift = right.GetInt();
            if (shift < 0)
            {
                throw new InterpretationException("The shift cannot be negative");
            }

            if (shift == 0)
            {
                return left;
            }

            if (left.ValueType == RpnConst.Type.Integer)
            {
                return new RpnInteger(PerformIntShift(left.GetInt(), shift));
            }

            if (left.ValueType == RpnConst.Type.String)
            {
                var str = left.GetString();
                var result = new string(str.Select(ch => PerformCharShift(ch, shift)).ToArray());
                return new RpnString(result);
            }

            throw new InterpretationException("Unexpected type of the left operand");
        }

        protected abstract int PerformIntShift(int number, int shift);
        protected abstract char PerformCharShift(char ch, int shift);
    }
}