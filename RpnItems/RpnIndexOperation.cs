namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an operation, that corresponds to index
    /// (get array item, check dictionary key existence etc.).
    /// </summary>
    public abstract class RpnIndexOperation : RpnBinaryOperation
    {
        public RpnIndexOperation(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override sealed int Priority => RpnOperation.IndexatorPriority;

        /// <inheritdoc/>
        protected override sealed RpnConst GetResultCore(RpnConst array, RpnConst index)
        {
            if (array.ValueType == RpnConst.Type.String)
            {
                var str = array.GetString();
                return GetResultForString(str, index);
            }

            if (array.ValueType == RpnConst.Type.Variable)
            {
                return GetResultForVariable(array, index);
            }

            throw new InterpretationException(
                "Can perform index operation only for a variable or a string");
        }

        /// <summary>
        /// Returns the result of the operation for a string.
        /// </summary>
        /// <param name="str">String operand.</param>
        /// <param name="index">Index operand.</param>
        /// <returns>Result of the operation for string.</returns>
        protected abstract RpnConst GetResultForString(string str, RpnConst index);

        /// <summary>
        /// Returns the result of the operation for a variable.
        /// </summary>
        /// <param name="variable">Variable operand.</param>
        /// <param name="index">Index operand.</param>
        /// <returns>Result of the operation for a variable.</returns>
        protected abstract RpnConst GetResultForVariable(RpnConst variable, RpnConst index);

        /// <summary>
        /// Constructs the name of the array item by the collection name and its index.
        /// </summary>
        protected static string GetIndexedName(string arrayName, RpnConst index)
            => GetIndexedPrefix(arrayName) + $"{index.ValueType.ToString()[0]}#{index.GetString()}";

        /// <summary>
        /// Constructs the prefix of the array item name.
        /// </summary>
        protected static string GetIndexedPrefix(string arrayName)
            => $"{arrayName}#";
    }
}