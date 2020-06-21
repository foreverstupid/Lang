using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    /// <summary>
    /// Help enumerator for the token collection.
    /// </summary>
    public class TokenEnumerator
    {
        private static readonly string[] UnarOperations = new[] { "-", "!", "+" };
        private static readonly string[] BinarOperations =
            new[] { "+", "-", "/", "*", "/", "&", "|", ">", "<", "~", "%", "^" };

        private readonly IEnumerator<Token> tokens;

        public TokenEnumerator(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.GetEnumerator();
            MoveNext(); // make the first step
        }

        /// <summary>
        /// Whether the token collection is finished.
        /// </summary>
        public bool IsFinished { get; private set; } = false;

        /// <summary>
        /// The current token of the enumerator or the last token if the enumerator is finished.
        /// </summary>
        public Token CurrentOrLast { get; private set; }

        /// <summary>
        /// Moves to the next token in the collection.
        /// </summary>
        public void MoveNext()
        {
            if (tokens.MoveNext())
            {
                CurrentOrLast = tokens.Current;
            }
            else
            {
                IsFinished = true;
            }
        }

        /// <summary>
        /// Checks whether the current token has the given value or not.
        /// </summary>
        /// <param name="value">The comparing value.</param>
        public bool CurrentTokenValueIs(string value)
        {
            if (IsFinished)
            {
                return false;
            }

            return CurrentOrLast.Value == value;
        }

        /// <summary>
        /// Checks whether the current token has the given type or not.
        /// </summary>
        /// <param name="value">The comparing type.</param>
        public bool CurrentTokenTypeIs(Token.Type type)
        {
            if (IsFinished)
            {
                return false;
            }

            return CurrentOrLast.TokenType == type;
        }

        /// <summary>
        /// Checks whether the current token is an unary operation.
        /// </summary>
        public bool CurrentTokenIsUnaryOperation()
        {
            return
                CurrentOrLast.TokenType == Token.Type.Separator &&
                UnarOperations.Contains(CurrentOrLast.Value);
        }

        /// <summary>
        /// Checks whether the current token is a binary operation.
        /// </summary>
        public bool CurrentTokenIsBinaryOperation()
        {
            return
                CurrentOrLast.TokenType == Token.Type.Separator &&
                BinarOperations.Contains(CurrentOrLast.Value);
        }
    }
}