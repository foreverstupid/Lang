using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN that represents conditional jump to the certain label.
    /// </summary>
    public class RpnIfGoto : Rpn
    {
        public RpnIfGoto(Token token)
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