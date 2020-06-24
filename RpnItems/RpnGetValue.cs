using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents dereference (getting value of the variable).
    /// </summary>
    public class RpnGetValue : RpnUnaryOperation
    {
        public RpnGetValue(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.DereferencePriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
        {
            // TODO: positional args
            if (operand.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot get a value of non-variable type");
            }

            throw new System.NotImplementedException();
        }
    }
}