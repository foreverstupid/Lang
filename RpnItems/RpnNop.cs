using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// Help RPN item that do nothing.
    /// </summary>
    public class RpnNop : Rpn
    {
        /// <inheritdoc/>
        public override LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd
        )
            => currentCmd.Next;
    }
}