using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// The base class of all Reverse Polish Notation items.
    /// </summary>
    public abstract class Rpn
    {
        public Rpn(Token token) => Token = token;

        public Rpn()
        {
        }

        public Token Token { get; } = null;

        /// <summary>
        /// Evaluates the command that is described by the RPN.
        /// </summary>
        /// <param name="stack">The stack of the interpretation.</param>
        /// <param name="currentCmd">The current command.</param>
        /// <returns>The next command.</returns>
        public abstract LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd);

        /// <inheritdoc/>
        public override string ToString()
            => this.GetType().Name +
               (Token is null ? "" : $": {Token.Value} ({Token.Line}:{Token.StartPosition})");
    }
}