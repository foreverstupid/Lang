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

        /// <summary>
        /// Variable value of the item.
        /// </summary>
        public Variable VariableValue => variableValue;

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference"
            );

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference"
            );

        /// <inheritdoc/>
        public override string GetString() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference"
            );

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException(
                "The variable itself cannot be casted to bool. Use dereference first"
            );
    }
}