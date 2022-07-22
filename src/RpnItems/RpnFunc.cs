using Lang.Exceptions;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a function.
    /// </summary>
    public sealed class RpnFunc : RpnConst
    {
        private readonly EntityName name;

        public RpnFunc(string name)
        {
            this.name = new EntityName(name);
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
            => throw new InterpretationException("Cannot cast function to string");

        /// <inheritdoc/>
        public override EntityName GetName()
            => name;

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnFunc) + ": " + name;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetName() == this.GetName();
    }
}