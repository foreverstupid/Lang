using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment when assignable value is right and
    /// assigning is left one. Returns assignable value to the stack.
    /// </summary>
    public sealed class RpnInsert : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;
        private readonly RpnBinaryOperation additionalOperation;

        public RpnInsert(
            Token token,
            IDictionary<EntityName, RpnConst> variables,
            RpnBinaryOperation additionalOperation = null)
            : base(token)
        {
            this.variables = variables;
            this.additionalOperation = additionalOperation;
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AssignmentPriority;

        /// <inheritdoc/>
        public override bool HasLessPriorityThan(RpnOperation anotherOperation)
        {
            if (anotherOperation is RpnInsert)
            {
                return true;
            }

            return base.HasLessPriorityThan(anotherOperation);
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (right.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot assign to the non-variable value");
            }

            if (!(additionalOperation is null))
            {
                if (!variables.TryGetValue(right.GetName(), out var value))
                {
                    throw new InterpretationException(
                        "Cannot use non-existing variable for complex assignment");
                }

                left = additionalOperation.Perform(left, value);
            }

            variables[right.GetName()] = left;
            return right;
        }
    }
}