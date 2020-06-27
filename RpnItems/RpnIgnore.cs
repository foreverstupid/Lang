using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that ignores the last stack value, removing it from stack.
    /// </summary>
    public class RpnIgnore : RpnSuccessive
    {
        /// <inheritdoc/>
        protected override void EvalCore(Stack<RpnConst> stack)
            => stack.Pop();
    }
}