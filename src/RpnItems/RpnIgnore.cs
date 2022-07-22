using System.Collections.Generic;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that ignores the last stack value, removing it from stack.
    /// </summary>
    public sealed class RpnIgnore : RpnSuccessive
    {
        public RpnIgnore()
        {
        }

        public RpnIgnore(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override void EvalCore(Stack<RpnConst> stack)
            => stack.Pop();
    }
}