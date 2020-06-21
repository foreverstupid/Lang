using System;
using System.Collections.Generic;
using System.Text;

namespace Lang
{
    /// <summary>
    /// Performs lexical analysis of the source code.
    /// </summary>
    public sealed class LexicalParser
    {
        private static readonly string Separators = ";,=$+-*/%><!|&~()[]{}:";
        private readonly Dictionary<State, Func<char, Token>> stateHandlers;

        private readonly StringBuilder tokenValue = new StringBuilder();
        private int tokenStartPosition;
        private int currentPosition = 1;
        private int currentLine = 1;
        private State state = State.None;

        private Token extraToken = null;

        public LexicalParser()
        {
            // every handler returns the new token if it was constructed, or null otherwise
            stateHandlers = new Dictionary<State, Func<char, Token>>()
            {
                [State.None] = None,
                [State.Integer] = Integer,
                [State.Float] = Float,
                [State.Identifier] = Identifier,
                [State.Label] = Label,
                [State.String] = String,
                [State.Slash] = Slash,
                [State.Comment] = Comment,
            };
        }

        /// <summary>
        /// Analysis states.
        /// </summary>
        enum State
        {
            None,
            Integer,
            Float,
            String,
            Slash,
            Comment,
            Identifier,
            Label,
        }

        /// <summary>
        /// Reads a new character of the source and if the token is generated returns it,
        /// otherwise returns null.
        /// </summary>
        /// <param name="character">The current character of the source code.</param>
        /// <returns>The new token if it has been generated or null otherwise.</returns>
        public Token GetNextToken(int character)
        {
            var newToken = extraToken;  // the last extra token should be returned on the current invocation

            // process the current character
            char nextChar = character == -1 ? ' ' : (char)character;
            var currentToken = stateHandlers[state](nextChar);

            // if we had an extra token then we return it on the current invocation
            // and the current token becomes the extra one (if it is presented).
            // Otherwise we just return the current token
            if (!(newToken is null))
            {
                extraToken = currentToken;
            }
            else
            {
                newToken = currentToken;
            }

            ChangePositionInfo();   // renew position information
            return newToken;

            /// <summary>
            /// Changes the current position information.
            /// </summary>
            void ChangePositionInfo()
            {
                if (nextChar == '\n')
                {
                    currentLine++;
                    currentPosition = 1;
                }
                else
                {
                    currentPosition++;
                }
            }
        }

        private Token None(char character)
        {
            tokenStartPosition = currentPosition;
            if (char.IsDigit(character))
            {
                state = State.Integer;
                tokenValue.Append(character);
            }
            else if (character == '\"')
            {
                state = State.String;
            }
            else if (char.IsLetter(character) || character == '_')
            {
                state = State.Identifier;
                tokenValue.Append(character);
            }
            else if (character == '@')
            {
                state = State.Label;
                tokenValue.Append(character);
            }
            else if (character == '#')
            {
                state = State.Comment;
            }
            else if (Separators.Contains(character))
            {
                tokenValue.Append(character);
                return OnNewToken(Token.Type.Separator);
            }
            else if (char.IsWhiteSpace(character))
            {
            }
            else
            {
                throw new ArgumentException($"Unexpected character '{character}'");
            }

            return null;
        }

        private Token Integer(char character)
        {
            if (character == '.')
            {
                state = State.Float;
                tokenValue.Append(character);
            }
            else if (char.IsWhiteSpace(character))
            {
                return OnNewToken(Token.Type.Integer);
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(character, Token.Type.Integer);
            }
            else if (char.IsDigit(character))
            {
                tokenValue.Append(character);
            }
            else
            {
                throw new ArgumentException($"Integer number token contains unexpected character '{character}'");
            }

            return null;
        }

        private Token Float(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return OnNewToken(Token.Type.Float);
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(character, Token.Type.Float);
            }
            else if (char.IsDigit(character))
            {
                tokenValue.Append(character);
            }
            else
            {
                throw new ArgumentException($"Float number token contains unexpected character '{character}'");
            }

            return null;
        }

        private Token String(char character)
        {
            if (character == '\\')
            {
                state = State.Slash;
            }
            else if (character == '\"')
            {
                return OnNewToken(Token.Type.String);
            }
            else
            {
                tokenValue.Append(character);
            }

            return null;
        }

        private Token Slash(char character)
        {
            tokenValue.Append(character);
            return null;
        }

        private Token Comment(char character)
        {
            if (character == '\n')
            {
                state = State.None;
            }

            return null;
        }

        private Token Identifier(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return OnNewToken(Token.Type.Identifier);
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(character, Token.Type.Identifier);
            }
            else if (char.IsLetterOrDigit(character) || character == '_')
            {
                tokenValue.Append(character);
            }
            else
            {
                throw new ArgumentException($"Identifier contains unexpected character '{character}'");
            }

            return null;
        }

        private Token Label(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return OnNewToken(Token.Type.Label);
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(character, Token.Type.Label);
            }
            else if (char.IsLetterOrDigit(character) || character == '_')
            {
                tokenValue.Append(character);
            }
            else
            {
                throw new ArgumentException($"Label contains unexpected character '{character}'");
            }

            return null;
        }

        /// <summary>
        /// Returns the token of the given type, using the current token info.
        /// </summary>
        private Token GetNewToken(Token.Type type)
        {
            var value = tokenValue.ToString();
            tokenValue.Clear();
            return new Token(value, type, tokenStartPosition, currentLine);
        }

        /// <summary>
        /// Actions on a new token creating.
        /// </summary>
        /// <returns>A new created token.</returns>
        private Token OnNewToken(Token.Type tokenType)
        {
            state = State.None;
            return GetNewToken(tokenType);
        }

        /// <summary>
        /// Actions on the extra token case.
        /// </summary>
        /// <param name="character">The current character.</param>
        /// <param name="currentTokenType">The current token type.</param>
        /// <returns>The current token.</returns>
        /// <remarks>It is used when a new character is a separator and breaks the last token. In this case,
        /// one character produces two tokens, but we can return only one token a time.null So, we should
        /// keep an extra token and return it on the next invocation.</remarks>
        private Token OnExtraToken(char character, Token.Type currentTokenType)
        {
            var token = OnNewToken(currentTokenType);
            extraToken = stateHandlers[state](character);
            return token;
        }
    }
}