using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents addition.
    /// </summary>
    public class RpnAdd : RpnOperation
    {
        public RpnAdd(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.AddSubPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            var right = stack.Pop();
            var left = stack.Pop();

            return left.ValueType switch
            {
                RpnConst.Type.Float => new RpnFloat(left.GetFloat() + right.GetFloat()),
                RpnConst.Type.Integer => new RpnInteger(left.GetInt() + right.GetInt()),
                RpnConst.Type.String => new RpnString(left.GetString() + right.GetString()),
                var type =>
                    throw new InterpretationException(
                        $"Unexpected type of the left operand: {type}"
                    )
            };
        }
    }
}