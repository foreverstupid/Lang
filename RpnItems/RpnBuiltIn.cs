namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a built-in function.
    /// </summary>
    public class RpnBuiltIn : RpnConst
    {
        private readonly string name;

        public RpnBuiltIn(Token token, string name)
            : base(token)
        {
            this.name = name;
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.BuiltIn;

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException(
                "Built-in function cannot be casted to float"
            );

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException(
                "Built-in function cannot be casted to int"
            );

        /// <inheritdoc/>
        public override string GetString() => name;

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException(
                "Built-in function cannot be casted to bool"
            );
    }
}