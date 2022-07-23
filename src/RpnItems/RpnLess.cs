using System.Linq;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents less comparision operation.
    /// </summary>
    public sealed class RpnLess : RpnLogicalOperation
    {
        public RpnLess(Token token, bool isReversed = false)
            : base(token, isReversed)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.ComparisionPriority;

        /// <inheritdoc/>
        protected override bool GetBoolResult(RpnConst left, RpnConst right)
            => left.ValueType switch
            {
                RpnConst.Type.Float => left.GetFloat() < right.GetFloat(),
                RpnConst.Type.Integer => left.GetInt() < right.GetInt(),
                RpnConst.Type.String => IsLess(left.GetString(), right.GetString()),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}")
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