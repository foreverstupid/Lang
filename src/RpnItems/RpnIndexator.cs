using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents array or dictionary indexing.
    /// </summary>
    public sealed class RpnIndexator : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;

        public RpnIndexator(Token token, IDictionary<EntityName, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        /// <inheritdoc/>
        protected override sealed int Priority => RpnOperation.IndexatorPriority;

        /// <inheritdoc/>
        protected override sealed RpnConst GetResultCore(RpnConst array, RpnConst operand)
        {
            if (array.ValueType == RpnConst.Type.String)
            {
                var str = array.GetString();
                return GetResultForString(str, operand);
            }

            if (array.ValueType == RpnConst.Type.Variable)
            {
                return GetResultForVariable(array, operand);
            }

            throw new InterpretationException(
                "Can perform index operation only for a variable or a string");
        }

        /// <summary>
        /// Constructs the name of the array item by the collection name and its index.
        /// </summary>
        public static EntityName GetIndexedName(EntityName arrayName, RpnConst index)
        {
            var value = GetIndexedPrefix(arrayName) + $"{index.ValueType.ToString()[0]}#{index.GetString()}";
            return new EntityName(value);
        }

        /// <summary>
        /// Constructs the prefix of the array item name.
        /// </summary>
        public static string GetIndexedPrefix(EntityName arrayName)
            => $"{arrayName}#";

        private RpnConst GetResultForString(string str, RpnConst index)
        {
            if (index.ValueType != RpnConst.Type.Integer)
            {
                throw new InterpretationException("Index of a string should be an integer number");
            }

            var idx = index.GetInt();
            if (idx < 0 || idx >= str.Length)
            {
                throw new InterpretationException("Index was out of range");
            }

            return new RpnString(str[idx]);
        }

        private RpnConst GetResultForVariable(RpnConst array, RpnConst index)
        {
            if (index.ValueType == RpnConst.Type.String ||
                index.ValueType == RpnConst.Type.Integer ||
                index.ValueType == RpnConst.Type.Float)
            {
                var indexedName = GetIndexedName(array.GetName(), index);
                return new RpnVar(indexedName, variables);
            }

            throw new InterpretationException(
                "Index of a variable should be a number or a string");
        }
    }
}