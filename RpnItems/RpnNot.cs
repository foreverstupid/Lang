using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents logical NOT operation.
    /// </summary>
    public class RpnNot : RpnUnaryOperation
    {
        public RpnNot(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.UnarOperationPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
            => new RpnInteger(!operand.GetBool());
    }
}