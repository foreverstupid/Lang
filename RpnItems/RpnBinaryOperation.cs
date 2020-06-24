using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a binary operation.
    /// </summary>
    public abstract class RpnBinaryOperation : RpnOperation
    {
        public RpnBinaryOperation(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            var right = stack.Pop();
            var left = stack.Pop();
            return GetResultCore(left, right);
        }

        /// <summary>
        /// The main calculations of the result.
        /// </summary>
        /// <param name="left">The left operand of the operation.</param>
        /// <param name="right">The right operand of the operation.</param>
        /// <returns>Result of the operation.</returns>
        protected abstract RpnConst GetResultCore(RpnConst left, RpnConst right);
    }
}