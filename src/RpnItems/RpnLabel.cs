using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN that represents a command label.
    /// </summary>
    public sealed class RpnLabel : RpnConst
    {
        private readonly EntityName name;

        public RpnLabel(string name)
        {
            this.name = new EntityName(name);
        }

        public RpnLabel(Token token, string name)
            : base(token)
        {
            this.name = new EntityName(name);
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.Label;

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException("Label cannot have a float value");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException("Label cannot have an integer value");

        /// <inheritdoc/>
        public override string GetString() =>
            throw new InterpretationException("Label cannot have string value");

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException("Label cannot have a bool value");

        /// <inheritdoc/>
        public override EntityName GetName() => name;

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnLabel) + ": " + name;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetName() == this.GetName();
    }
}