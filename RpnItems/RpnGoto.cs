using System;
using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents unconditional jump to the certain label.
    /// </summary>
    public class RpnGoto : Rpn
    {
        protected readonly IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels;

        public RpnGoto(IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels)
        {
            this.labels = labels;
        }

        public RpnGoto(Token token, IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels)
            : base(token)
        {
            this.labels = labels;
        }

        /// <inheritdoc/>
        public override LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd)
        {
            var label = stack.Pop();

            if (label.ValueType != RpnConst.Type.Label)
            {
                throw new ArgumentException("Label expected");
            }

            if (labels.TryGetValue(label.GetString(), out var command))
            {
                return command;
            }
            else
            {
                throw new ArgumentException("Label is uninitialized");
            }
        }
    }
}