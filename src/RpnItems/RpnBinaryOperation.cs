using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

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

        /// <summary>
        /// Performs the operation action aver the given operands.
        /// </summary>
        public RpnConst Perform(RpnConst left, RpnConst right)
            => GetResultCore(left, right);

        /// <inheritdoc/>
        protected override sealed RpnConst GetResult(Stack<RpnConst> stack)
        {
            var right = stack.Pop();
            var left = stack.Pop();

            if (left.ValueType == RpnConst.Type.None || right.ValueType == RpnConst.Type.None)
            {
                throw new InterpretationException(
                    "Cannot perform operations over the None value");
            }

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