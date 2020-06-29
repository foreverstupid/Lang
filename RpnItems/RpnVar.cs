namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a variable.
    /// </summary>
    public class RpnVar : RpnConst
    {
        private readonly string name;

        public RpnVar(string name)
        {
            this.name = name;
        }

        public RpnVar(Token token, string name)
            : base(token)
        {
            this.name = name;
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.Variable;

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
        public override string GetString() => name;

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException(
                "The variable itself cannot be casted to bool. Use dereference first"
            );

        /// <inheritdoc/>
        public override string ToString()
            => this.GetType().Name + ": " + name;
    }
}