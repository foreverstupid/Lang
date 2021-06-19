using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// Help RPN item that does nothing.
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