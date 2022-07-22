namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a built-in function.
    /// </summary>
    public sealed class RpnBuiltIn : RpnConst
    {
        private readonly EntityName name;

        public RpnBuiltIn(EntityName name)
        {
            this.name = name;
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.BuiltIn;

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException(
                "Built-in function realization cannot be casted to float");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException(
                "Built-in function realization cannot be casted to int");

        /// <inheritdoc/>
        public override string GetString() =>
             throw new InterpretationException(
                "Built-in function realization cannot be casted to string");

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException(
                "Built-in function realization cannot be casted to bool");

        /// <inheritdoc/>
        public override EntityName GetName() => name;

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnBuiltIn) + ": " + name;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetName() == this.GetName();
    }
} 