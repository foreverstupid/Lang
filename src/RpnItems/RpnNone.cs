namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents nonexistent value (using as return
    /// value of the cycle that hasn't performed any iteration)
    /// </summary>
    public sealed class RpnNone : RpnConst
    {
        /// <inheritdoc/>
        public override Type ValueType => RpnConst.Type.None;

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException("Cannot cast None value to bool");

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException("Cannot cast None value to float");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException("Cannot cast None value to integer");

        /// <inheritdoc/>
        public override string GetString() =>
            throw new InterpretationException("Cannot cast None value to string");

        /// <inheritdoc/>
        public override string ToString()
            => "None";

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => true;
    }
}