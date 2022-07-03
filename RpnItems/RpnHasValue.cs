using System.Collections.Generic;
using System.Linq;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents checking of the index existence.
    /// </summary>
    public sealed class RpnHasValue : RpnIndexOperation
    {
        private readonly IDictionary<string, RpnConst> variables;

        public RpnHasValue(Token token, IDictionary<string, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultForString(string str, RpnConst index)
        {
            if (index.ValueType != RpnConst.Type.String)
            {
                throw new InterpretationException(
                    "Right-hand operand for a string check contains operation should be a string");
            }

            bool contains = str.Contains(index.GetString());
            return RpnConst.Bool(contains);
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultForVariable(RpnConst variable, RpnConst value)
        {
            var variableName = GetIndexedPrefix(variable.GetString());
            var foundItems = variables
                .Where(item =>
                    item.Key.StartsWith(variableName) &&
                    item.Value.HasSameValue(value))
                .ToArray();

            return RpnConst.Bool(foundItems.Length > 0);
        }
    }
}