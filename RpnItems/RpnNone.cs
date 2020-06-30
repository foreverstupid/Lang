namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents nonexistent value (using as return
    /// value of the cycle that hasn't performed any iteration)
    /// </summary>
    public class RpnNone : RpnConst
    {
        /// <inheritdoc/>
        public override Type ValueType => RpnConst.Type.None;

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException("Cannot cast none value to bool");

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException("Cannot cast none value to bool");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException("Cannot cast none value to bool");

        /// <inheritdoc/>
        public override string GetString() =>
            throw new InterpretationException("Cannot cast none value to bool");

        /// <inheritdoc/>
        public override string ToString() =>
            "None";
    }
}