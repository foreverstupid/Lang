using System.Collections.Generic;
using System.Linq;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an operation for check whether the given
    /// item is contained in the given object.
    /// </summary>
    public sealed class RpnIn : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;

        public RpnIn(Token token, IDictionary<EntityName, RpnConst> variables)
            : base(token)
        {
            this.variables = variables;
        }

        protected override int Priority => RpnOperation.ComparisionPriority;

        protected override RpnConst GetResultCore(RpnConst item, RpnConst complex)
        {
            if (complex.ValueType == RpnConst.Type.String)
            {
                var str = complex.GetString();
                return GetResultForString(item, str);
            }

            if (complex.ValueType == RpnConst.Type.Variable)
            {
                return GetResultForVariable(item, complex);
            }

            throw new InterpretationException(
                "Can perform in-check operation only for a variable or a string");
        }

        private RpnConst GetResultForString(RpnConst item, string str)
        {
            if (item.ValueType != RpnConst.Type.String)
            {
                throw new InterpretationException(
                    "Left-hand operand of a string in-check operation should be a string");
            }

            bool contains = str.Contains(item.GetString());
            return RpnConst.Bool(contains);
        }

        /// <inheritdoc/>
        private RpnConst GetResultForVariable(RpnConst variable, RpnConst array)
        {
            var arrayPrefix = RpnIndexator.GetIndexedPrefix(array.GetName());
            var foundItems = variables
                .Where(item =>
                    item.Key.Value.StartsWith(arrayPrefix) &&
                    item.Value.HasSameValue(variable))
                .ToArray();

            if (foundItems.Length == 0)
            {
                return RpnConst.False;
            }

            return new RpnVar(foundItems[0].Key, variables);
        }
    }
}