using System.Text;
using System.Collections.Generic;
using Lang.Exceptions;
using Lang.Pipeline;
using Lang.Content;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents array or dictionary indexing.
    /// </summary>
    public sealed class RpnIndexator : RpnBinaryOperation
    {
        private const char IndexDelimiter = '#';
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
            var indexType = index.ValueType.ToString()[0];
            var value =
                GetIndexedPrefix(arrayName) +
                $"{indexType}{IndexDelimiter}{index.GetString()}";

            return new EntityName(value);
        }

        /// <summary>
        /// Constructs the prefix of the array item name.
        /// </summary>
        public static string GetIndexedPrefix(EntityName arrayName)
            => $"{arrayName}{IndexDelimiter}";

        /// <summary>
        /// Returns the readable name of the indexed variable.
        /// </summary>
        /// <returns></returns>
        public static string GetReadableName(string indexedName)
        {
            var parts = indexedName.Split(IndexDelimiter);
            var builder = new StringBuilder(parts[0]);
            for (int i = 1; i < parts.Length; i += 2)
            {
                builder.Append(Syntax.IndexatorStart);
                if (parts[i][0] == RpnConst.Type.String.ToString()[0])
                {
                    builder.Append('"');
                    builder.Append(parts[i + 1]);
                    builder.Append('"');
                }
                else
                {
                    builder.Append(parts[i + 1]);
                }

                builder.Append(Syntax.IndexatorEnd);
            }

            return builder.ToString();
        }

        private RpnConst GetResultForString(string str, RpnConst index)
        {
            if (index.ValueType == RpnConst.Type.String &&
                index.GetString() == Syntax.StringLengthPropertyName)
            {
                return new RpnInteger(str.Length);
            }

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
            if (RpnConst.MainTypes.HasFlag(index.ValueType))
            {
                var indexedName = GetIndexedName(array.GetName(), index);
                return new RpnVar(indexedName, variables);
            }

            throw new InterpretationException(
                "Index of a variable should be a number or a string");
        }
    }
}