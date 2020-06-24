using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents assignment some value to the variable.
    /// </summary>
    public class RpnAssign : RpnBinaryOperation
    {
        public RpnAssign(Token token)
            : base(token)
        { 
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

            var variable = left as RpnVar;
            variable.VariableValue.Set(right);
            return right;
        }
    }
}