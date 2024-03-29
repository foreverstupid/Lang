using System.Collections.Generic;
using System.Linq;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an operation for check whether the given
    /// item is contained in the given object.
    /// </summary>
    public sealed class RpnIn : RpnBinaryOperation
    {
        private readonly IDictionary<EntityName, RpnConst> variables;
        private readonly bool isReversed;

        public RpnIn(
            Token token,
            IDictionary<EntityName, RpnConst> variables,
            bool isReversed = false)
            : base(token)
        {
            this.variables = variables;
            this.isReversed = isReversed;
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
            if (isReversed)
            {
                contains = !contains;
            }

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
                return isReversed ? RpnConst.True : RpnConst.False;
            }

            return isReversed ? RpnConst.False : new RpnVar(foundItems[0].Key, variables);
        }
    }
}