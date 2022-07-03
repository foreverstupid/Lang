namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an operation, that corresponds to the indexing key.
    /// </summary>
    public abstract class RpnIndexKeyOperation : RpnIndexOperation
    {
        public RpnIndexKeyOperation(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override sealed RpnConst GetResultForString(string str, RpnConst index)
        {
            if (index.ValueType != RpnConst.Type.Integer)
            {
                throw new InterpretationException("Index of a string should be an integer number");
            }

            var idx = index.GetInt();
            return GetIntIndexResultForString(str, idx);
        }

        /// <inheritdoc/>
        protected override sealed RpnConst GetResultForVariable(RpnConst variable, RpnConst index)
        {
            if (index.ValueType == RpnConst.Type.String ||
                index.ValueType == RpnConst.Type.Integer ||
                index.ValueType == RpnConst.Type.Float)
            {
                var indexedName = GetIndexedName(variable.GetString(), index);
                return GetResultForIndexedVariable(indexedName);
            }

            throw new InterpretationException(
                "Index of a variable should be a number or a string");
        }

        /// <summary>
        /// Returns result of the int-indexing operation for a string.
        /// </summary>
        /// <param name="str">String operand.</param>
        /// <param name="index">Integer index.</param>
        /// <returns>Result of the int-indexing operation for a string.</returns>
        protected abstract RpnConst GetIntIndexResultForString(string str, int index);

        /// <summary>
        /// Returns result for the variable, whose indexed name is given.
        /// </summary>
        /// <param name="indexedName">The indexed name of the varibale.</param>
        /// <returns>Result of the operation for the variable.</returns>
        protected abstract RpnConst GetResultForIndexedVariable(string indexedName);
    }
}