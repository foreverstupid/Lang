using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a variable.
    /// </summary>
    public sealed class RpnVar : RpnConst
    {
        private readonly IDictionary<EntityName, RpnConst> variables;
        private readonly EntityName name;

        public RpnVar(EntityName name, IDictionary<EntityName, RpnConst> variables)
        {
            this.name = name;
            this.variables = variables;
        }

        public RpnVar(string name, IDictionary<EntityName, RpnConst> variables)
        {
            this.name = new EntityName(name);
            this.variables = variables;
        }

        public RpnVar(Token token, string name, IDictionary<EntityName, RpnConst> variables)
            : base(token)
        {
            this.name = new EntityName(name);
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
        public override string GetString() =>
            throw new InterpretationException(
                "Cannot direct get the variable value. Use dereference");

        /// <inheritdoc/>
        public override EntityName GetName()
            => name;

        /// <inheritdoc/>
        public override bool GetBool()
            => variables.ContainsKey(name);

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnVar) + ": " + name;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetName() == this.GetName();
    }
}