namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a function.
    /// </summary>
    public class RpnFunc : RpnConst
    {
        private readonly string name;

        public RpnFunc(string name)
        {
            this.name = name;
        }

        /// <inheritdoc/>
        public override Type ValueType => RpnConst.Type.Func;

        /// <inheritdoc/>
        public override bool GetBool()
            => throw new InterpretationException("Cannot cast function to bool");

        /// <inheritdoc/>
        public override double GetFloat()
            => throw new InterpretationException("Cannot cast function to float");

        /// <inheritdoc/>
        public override int GetInt()
            => throw new InterpretationException("Cannot cast function to int");

        /// <inheritdoc/>
        public override string GetString()
            => name;

        /// <inheritdoc/>
        public override string ToString()
            => this.GetType().Name + ": " + name;
    }
}