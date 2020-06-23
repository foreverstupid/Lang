using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents some value.
    /// </summary>
    public abstract class RpnConst : RpnSuccessive
    {
        public RpnConst()
            : base (null)
        {
        }

        public RpnConst(Token token)
            : base(token)
        {
        }

        /// <summary>
        /// The value of the item.
        /// </summary>
        public abstract object Value { get; }

        /// <inheritdoc/>
        protected override void EvalCore(Stack<RpnConst> stack)
            => stack.Push(this);
    }
}