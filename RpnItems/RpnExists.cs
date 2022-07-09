using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that checks variable existence.
    /// </summary>
    public sealed class RpnExists : RpnUnaryOperation
    {
        private readonly IDictionary<string, RpnConst> variables;

        public RpnExists(Token token, IDictionary<string, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
        {
            if (operand.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException(
                    "Existence check is allowed only for variables");
            }

            var varName = operand.GetString();
            bool isExist = variables.ContainsKey(varName);
            return RpnConst.Bool(isExist);
        }
    }
}