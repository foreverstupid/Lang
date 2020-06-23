using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical NOT operation.
    /// </summary>
    public class RpnNot : RpnOperation
    {
        public RpnNot(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.UnarOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}