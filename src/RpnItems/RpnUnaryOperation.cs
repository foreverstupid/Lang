using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

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
        public override bool HasLessPriorityThan(RpnOperation anotherOperation)
        {
            if (anotherOperation.GetType() == this.GetType())
            {
                return true;
            }

            return base.HasLessPriorityThan(anotherOperation);
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.UnarOperationPriority;

        /// <inheritdoc/>
        protected override sealed RpnConst GetResult(Stack<RpnConst> stack)
        {
            var operand = stack.Pop();
            if (operand.ValueType == RpnConst.Type.None)
            {
                throw new InterpretationException(
                    "Cannot perform operations over the None value"
                );
            }

            return GetResultCore(operand);
        }

        /// <summary>
        /// The main calculations of the result.
        /// </summary>
        /// <param name="operand">The operand of the operation.</param>
        /// <returns>Result of the operation.</returns>
        protected abstract RpnConst GetResultCore(RpnConst operand);
    }
}