using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents equality comparision.
    /// </summary>
    public sealed class RpnEqual : RpnBinaryOperation
    {
        public RpnEqual(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (left.ValueType == RpnConst.Type.Variable ||
                left.ValueType == RpnConst.Type.Func ||
                left.ValueType == RpnConst.Type.BuiltIn ||
                left.ValueType == RpnConst.Type.Label)
            {
                bool equal = right.ValueType == left.ValueType && right.GetName() == left.GetName();
                return RpnConst.Bool(equal);
            }

            return left.ValueType switch
            {
                RpnConst.Type.Float => RpnConst.Bool(left.GetFloat() == right.GetFloat()),
                RpnConst.Type.Integer => RpnConst.Bool(left.GetInt() == right.GetInt()),
                RpnConst.Type.String => RpnConst.Bool(left.GetString() == right.GetString()),
                _ => RpnConst.False
            };
        }
    }
}