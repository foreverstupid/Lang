using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents checking of the index existence.
    /// </summary>
    public sealed class RpnHasIndex : RpnIndexKeyOperation
    {
        private readonly IDictionary<string, RpnConst> variables;

        public RpnHasIndex(Token token, IDictionary<string, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override RpnConst GetIntIndexResultForString(string str, int index)
            => RpnConst.Bool(index >= 0 && index < str.Length);

        /// <inheritdoc/>
        protected override RpnConst GetResultForIndexedVariable(string indexedName)
            => RpnConst.Bool(variables.ContainsKey(indexedName));
    }
}