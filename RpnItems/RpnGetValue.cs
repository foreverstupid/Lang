using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents dereference (getting value of the variable).
    /// </summary>
    public sealed class RpnGetValue : RpnUnaryOperation
    {
        private readonly IReadOnlyDictionary<string, RpnConst> variables;

        public RpnGetValue(Token token, IReadOnlyDictionary<string, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.DereferencePriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
        {
            if (operand.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot get a value of a non-variable entity");
            }

            if (variables.TryGetValue(operand.GetString(), out var value))
            {
                return value;
            }
            else
            {
                throw new InterpretationException($"The variable '{operand.Token?.Value}' doesn't exist");
            }
        }
    }
}