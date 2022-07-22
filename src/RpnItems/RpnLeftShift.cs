using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents left shift operation.
    /// </summary>
    public sealed class RpnLeftShift : RpnShift
    {
        public RpnLeftShift(Token token)
            : base(token)
        {
        }

        protected override char PerformCharShift(char ch, int shift)
            => (char)(ch - shift);

        protected override int PerformIntShift(int number, int shift)
            => number << shift;
    }
}