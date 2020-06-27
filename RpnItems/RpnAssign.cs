using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment some value to the variable.
    /// </summary>
    public class RpnAssign : RpnBinaryOperation
    {
        private readonly IDictionary<string, RpnConst> variables;

        public RpnAssign(Token token, IDictionary<string, RpnConst> variables)
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

            variables[left.GetString()] = right;
            return right;
        }
    }
}