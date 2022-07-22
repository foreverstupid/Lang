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

        public RpnAssign(Token token, IDictionary<EntityName, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
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

            variables[left.GetName()] = right;
            return right;
        }
    }
}