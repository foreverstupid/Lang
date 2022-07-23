using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents bool-valued binary operation.
    /// </summary>
    public abstract class RpnLogicalOperation : RpnBinaryOperation
    {
        private readonly bool isReversed;

        public RpnLogicalOperation(Token token, bool isReversed = false)
            : base(token)
        {
            this.isReversed = isReversed;
        }

        protected sealed override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            var result = GetBoolResult(left, right);
            if (isReversed)
            {
                result = !result;
            }

            return RpnConst.Bool(result);
        }

        protected abstract bool GetBoolResult(RpnConst left, RpnConst right);
    }
}