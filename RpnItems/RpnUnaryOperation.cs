using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an unary operation.
    /// </summary>
    public abstract class RpnUnaryOperation : RpnOperation
    {
        public RpnUnaryOperation(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
            => GetResultCore(stack.Pop());

        /// <summary>
        /// The main calculations of the result.
        /// </summary>
        /// <param name="operand">The operand of the operation.</param>
        /// <returns>Result of the operation.</returns>
        protected abstract RpnConst GetResultCore(RpnConst operand);
    }
}