using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment some value to the variable.
    /// Returns assigning value to the stack.
    /// </summary>
    public sealed class RpnAssign : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;
        private readonly RpnBinaryOperation additionalOperation;

        public RpnAssign(
            Token token,
            IDictionary<EntityName, RpnConst> variables,
            RpnBinaryOperation additionalOperation = null)
            : base(token)
        {
            this.variables = variables;
            this.additionalOperation = additionalOperation;
        }

        /// <inheritdoc/>
        public override bool HasLessPriorityThan(RpnOperation anotherOperation)
        {
            if (anotherOperation is RpnAssign)
            {
                return true;
            }

            return base.HasLessPriorityThan(anotherOperation);
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AssignmentPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (left.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot assign to the non-variable value");
            }

            if (!(additionalOperation is null))
            {
                if (!variables.TryGetValue(left.GetName(), out var value))
                {
                    throw new InterpretationException(
                        "Cannot use non-existing variable for complex assignment");
                }

                right = additionalOperation.Perform(value, right);
            }

            variables[left.GetName()] = right;
            return right;
        }
    }
}