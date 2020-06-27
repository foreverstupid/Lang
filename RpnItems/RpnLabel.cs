using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN that represents a command label.
    /// </summary>
    public class RpnLabel : RpnConst
    {
        private readonly string name;

        public RpnLabel(string name)
        {
            this.name = name;
        }

        public RpnLabel(Token token, string name)
            : base(token)
        {
            this.name = name;
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
        public override string GetString() => name;

        /// <inheritdoc/>
        public override bool GetBool() =>
            throw new InterpretationException("Label cannot have a bool value");
    }
}