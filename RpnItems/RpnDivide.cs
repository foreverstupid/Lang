using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents division.
    /// </summary>
    public class RpnDivide : RpnOperation
    {
        public RpnDivide(Token token)
            : base(token)
        { 
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.MulDivPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResult(Stack<RpnConst> stack)
        {
            throw new System.NotImplementedException();
        }
    }
}