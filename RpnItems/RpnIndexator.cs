namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents array or dictionary indexing.
    /// </summary>
    public sealed class RpnIndexator : RpnIndexKeyOperation
    {
        public RpnIndexator(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override RpnConst GetIntIndexResultForString(string str, int index)
        {
            if (index < 0 || index >= str.Length)
            {
                throw new InterpretationException("Index was out of range");
            }

            return new RpnString(str[index]);
        }

        /// <inheritdoc/>
        protected override RpnConst GetResultForIndexedVariable(string indexedName)
            => new RpnVar(indexedName);
    }
}