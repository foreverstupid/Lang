using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment when assignable value is right and
    /// assigning is left one. Returns assignable value to the stack.
    /// </summary>
    public sealed class RpnRightAssign : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;

        public RpnRightAssign(Token token, IDictionary<EntityName, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AssignmentPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            if (right.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot assign to the non-variable value");
            }

            variables[right.GetName()] = left;
            return right;
        }
    }
}