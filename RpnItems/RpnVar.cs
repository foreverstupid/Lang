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
            ValueType = Type.Variable;
        }

        public RpnVar(Token token, Variable value)
            : base(token)
        {
            variableValue = value;
            ValueType = Type.Variable;
        }

        /// <inheritdoc/>
        public override Type ValueType { get; }

        /// <inheritdoc/>
        public override double GetFloat() => variableValue.FloatValue;

        /// <inheritdoc/>
        public override int GetInt() => variableValue.IntValue;

        /// <inheritdoc/>
        public override string GetString() => variableValue.StringValue;
    }
}