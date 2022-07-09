using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a variable.
    /// </summary>
    public sealed class RpnVar : RpnConst
    {
        private readonly IDictionary<string, RpnConst> variables;
        private readonly string name;

        public RpnVar(string name, IDictionary<string, RpnConst> variables)
        {
            this.name = name;
            this.variables = variables;
        }

        public RpnVar(Token token, string name, IDictionary<string, RpnConst> variables)
            : base(token)
        {
            this.name = name;
            this.variables = variables;
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.Variable;

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference");

        /// <inheritdoc/>
        public override string GetString() => name;

        /// <inheritdoc/>
        public override bool GetBool() =>
            variables.ContainsKey(name);

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnVar) + ": " + name;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetString() == this.GetString();
    }
}