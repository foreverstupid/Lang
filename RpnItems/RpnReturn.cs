using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that performs return from the lambda.
    /// </summary>
    /// <remark>
    /// We suppose that the stack at the lambda end looks like the following:
    ///        [return value]
    ///        [label to the returning command]
    /// Lambda always returns some value, and before lambda start there is always
    /// a returning label in the stack.
    /// </remark>
    public sealed class RpnReturn : RpnGoto
    {
        public RpnReturn(IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels)
            : base(labels)
        {
        }

        /// <inheritdoc/>
        public override LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd)
        {
            var retValue = stack.Pop();
            var nextCmd = base.Eval(stack, currentCmd);
            stack.Push(retValue);           // push the return value back
            return nextCmd;
        }
    }
}