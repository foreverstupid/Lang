namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a variable.
    /// </summary>
    public class RpnVar : RpnConst
    {
        private readonly Variable variableValue;

        public RpnVar(Variable value)
        {
            variableValue = value;
        }

        public RpnVar(Token token, Variable value)
            : base(token)
        {
            variableValue = value;
        }

        /// <inheritdoc/>
        public override object Value => variableValue.Value;
    }
}