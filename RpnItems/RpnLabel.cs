using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN that represents a command label.
    /// </summary>
    public class RpnLabel : RpnConst
    {
        public RpnLabel(LinkedListNode<Rpn> labeledCommand)
        {
            Value = labeledCommand;
        }

        public RpnLabel(Token token, LinkedListNode<Rpn> labeledCommand)
            : base(token)
        {
            Value = labeledCommand;
        }

        /// <inheritdoc/>
        public override object Value { get; }
    }
}