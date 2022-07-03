using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item whose evaluation shifts the current command by one step.
    /// </summary>
    public abstract class RpnSuccessive : Rpn
    {
        public RpnSuccessive()
        {
        }

        public RpnSuccessive(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        public override sealed LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd)
        {
            EvalCore(stack);
            return currentCmd.Next;
        }

        /// <summary>
        /// The main evaluating process of the <see cref="RpnSuccessive"/>.
        /// </summary>
        /// <param name="stack">The program stack.</param>
        protected abstract void EvalCore(Stack<RpnConst> stack);
    }
}