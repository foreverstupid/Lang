using System.Globalization;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that checks whether casting of the left operand to the type of the
    /// right one is possible.
    /// </summary>
    public class RpnCheckCast : RpnBinaryOperation
    {
        public RpnCheckCast(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.CastPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (right.ValueType != RpnConst.Type.String &&
                right.ValueType != RpnConst.Type.Float &&
                right.ValueType != RpnConst.Type.Integer &&
                left.ValueType != RpnConst.Type.String &&
                left.ValueType != RpnConst.Type.Float &&
                left.ValueType != RpnConst.Type.Integer)
            {
                return new RpnInteger(0);
            }

            if (left.ValueType == RpnConst.Type.String)
            {
                if (right.ValueType == RpnConst.Type.Integer)
                {
                    return
                        int.TryParse(
                            left.GetString(),
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out _)
                        ? new RpnInteger(1)
                        : new RpnInteger(0);
                }

                if (right.ValueType == RpnConst.Type.Float)
                {
                    return
                        double.TryParse(
                            left.GetString(),
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out _)
                        ? new RpnInteger(1)
                        : new RpnInteger(0);
                }
            }

            return new RpnInteger(1);
        }
    }
}