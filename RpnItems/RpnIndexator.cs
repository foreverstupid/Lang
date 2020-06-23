using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents indexing.
    /// </summary>
    public class RpnIndexator : RpnOperation
    {
        public RpnIndexator(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => 1000;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}