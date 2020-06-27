namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents indexing.
    /// </summary>
    public class RpnIndexator : RpnBinaryOperation
    {
        public RpnIndexator(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => 1000;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst array, RpnConst index)
        {
            if (array.ValueType == RpnConst.Type.String)
            {
                int idx = index.GetInt();
                string str = array.GetString();

                if (idx < 0 || idx >= str.Length)
                {
                    throw new InterpretationException("Index is out of range");
                }

                return new RpnString(str[idx].ToString());
            }

            if (array.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot index not a variable");
            }

            var indexedName = ConstructName(array.GetString(), index);
            return new RpnVar(indexedName);
        }

        /// <summary>
        /// Constructs the name of the array item by the collection name and its index.
        /// </summary>
        private string ConstructName(string arrayName, RpnConst index)
            => arrayName + "#" + index.ValueType.ToString() + index.GetString();
    }
}