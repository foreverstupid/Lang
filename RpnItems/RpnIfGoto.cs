using System;
using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN that represents conditional jump to the certain label.
    /// </summary>
    public sealed class RpnIfGoto : Rpn
    {
        private readonly IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels;

        public RpnIfGoto(Token token, IReadOnlyDictionary<string, LinkedListNode<Rpn>> labels)
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
            var condition = stack.Pop();

            if (label.ValueType != RpnConst.Type.Label)
            {
                throw new ArgumentException("Label expected");
            }

            if (labels.TryGetValue(label.GetString(), out var command))
            {
                return !condition.GetBool() ? command : currentCmd.Next;
            }
            else
            {
                throw new ArgumentException("Label is uninitialized");
            }
        }
    }
}