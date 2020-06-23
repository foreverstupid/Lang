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
            ValueType = Type.Label;
        }

        public RpnLabel(Token token, LinkedListNode<Rpn> labeledCommand)
            : base(token)
        {
            Value = labeledCommand;
            ValueType = Type.Label;
        }

        /// <inheritdoc/>
        public override Type ValueType { get; }

        /// <inheritdoc/>
        public override double GetFloat() =>
            throw new InterpretationException("Label cannot have a float value");

        /// <inheritdoc/>
        public override int GetInt() =>
            throw new InterpretationException("Label cannot have an integer value");

        /// <inheritdoc/>
        public override string GetString() =>
            throw new InterpretationException("Label cannot have a string value");

        /// <summary>
        /// Gets the labeled command.
        /// </summary>
        public LinkedListNode<Rpn> Value { get; }
    }
}