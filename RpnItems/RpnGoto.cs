using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents unconditional jump to the certain label.
    /// </summary>
    public class RpnGoto : Rpn
    {
        public RpnGoto()
        {
        }

        public RpnGoto(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        public override LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd)
        {
            throw new System.NotImplementedException();
        }
    }
}