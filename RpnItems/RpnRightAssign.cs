using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment when assignable value is right and
    /// assigning is left one. Returns assignable value to the stack.
    /// </summary>
    public class RpnRightAssign : RpnBinaryOperation
    {
        private readonly IDictionary<string, RpnConst> variables;

        public RpnRightAssign(Token token, IDictionary<string, RpnConst> variables)
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

            variables[right.GetString()] = left;
            return right;
        }
    }
}