using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents equality comparision.
    /// </summary>
    public sealed class RpnEqual : RpnLogicalOperation
    {
        public RpnEqual(Token token, bool isReversed = false)
            : base(token, isReversed)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override bool GetBoolResult(RpnConst left, RpnConst right)
        {
            if (left.ValueType == RpnConst.Type.Variable ||
                left.ValueType == RpnConst.Type.Func ||
                left.ValueType == RpnConst.Type.BuiltIn ||
                left.ValueType == RpnConst.Type.Label)
            {
                bool equal = right.ValueType == left.ValueType && right.GetName() == left.GetName();
                return equal;
            }

            return left.ValueType switch
            {
                RpnConst.Type.Float => left.GetFloat() == right.GetFloat(),
                RpnConst.Type.Integer => left.GetInt() == right.GetInt(),
                RpnConst.Type.String => left.GetString() == right.GetString(),
                _ => false
            };
        }
    }
}