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
                "Cannot get the value of a built-in function"
            );

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException(
                "Cannot get the value of a built-in function"
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