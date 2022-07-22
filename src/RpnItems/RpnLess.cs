using System.Linq;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents less comparision operation.
    /// </summary>
    public sealed class RpnLess : RpnBinaryOperation
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
                RpnConst.Type.Float => RpnConst.Bool(left.GetFloat() < right.GetFloat()),
                RpnConst.Type.Integer => RpnConst.Bool(left.GetInt() < right.GetInt()),
                RpnConst.Type.String => RpnConst.Bool(IsLess(left.GetString(), right.GetString())),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };

        private bool IsLess(string s1, string s2)
        {
            for (int i = 0; i < s1.Length && i < s2.Length; i++)
            {
                if (s1[i] < s2[i])
                {
                    return true;
                }

                if (s1[i] > s2[i])
                {
                    return false;
                }
            }

            return s1.Length < s2.Length;
        }
    }
}