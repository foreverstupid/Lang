using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lang
{
    /// <summary>
    /// Performs lexical analysis of the source code.
    /// </summary>
    public sealed class LexicalParser
    {
        private static readonly string Separators = ";,=$.+-*/%><!|&~()[]{}:?";
        private readonly Dictionary<State, Func<char, IEnumerable<Token>>> stateHandlers;

        private readonly StringBuilder tokenValue = new StringBuilder();
        private int tokenStartPosition;
        private int currentPosition = 1;
        private int currentLine = 1;
        private State state = State.None;

        private string hexSymbol = "";
        private bool isInterpolation = false;

        public LexicalParser()
        {
            // every handler returns the new token if it was constructed, or null otherwise
            stateHandlers = new Dictionary<State, Func<char, IEnumerable<Token>>>()
            {
                [State.None] = None,
                [State.Integer] = Integer,
                [State.Float] = Float,
                [State.Identifier] = Identifier,
                [State.String] = String,
                [State.Slash] = Slash,
                [State.HexSymbol] = HexSymbol,
                [State.Comment] = Comment,
                [State.RightAssign] = RightAssign,
                [State.Apply] = Apply,
                [State.Field] = Field,
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
            HexSymbol,
            Comment,
            Identifier,
            RightAssign,
            Apply,
            Field,
        }

        /// <summary>
        /// Parses a given source code into the collection of sequential tokens.
        /// </summary>
        /// <param name="src">The program source code.</param>
        /// <returns>The tokens that was generated from the given source code.</returns>
        public IEnumerable<Token> Parse(string src)
        {
            // helps in cases when the source code ends with a separator
            src += " ";

            var result = new List<Token>();
            foreach (var ch in src)
            {
                var newTokens = stateHandlers[state](ch);
                if (newTokens != null)
                {
                    result.AddRange(newTokens);
                }

                ChangePositionInfo(ch);   // renew position information
            }

            if (state == State.String)
            {
                throw new ArgumentException("String literal end is not reached");
            }

            if (isInterpolation)
            {
                throw new ArgumentException("Interpolation string literal end is not reached");
            }

            return result;

            /// <summary>
            /// Changes the current position information.
            /// </summary>
            void ChangePositionInfo(char nextChar)
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

        private IEnumerable<Token> None(char character)
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
            else if (character == '#')
            {
                state = State.Comment;
            }
            else if (character == '-')
            {
                state = State.RightAssign;
                tokenValue.Append(character);
            }
            else if (character == '=')
            {
                state = State.Apply;
                tokenValue.Append(character);
            }
            else if (character == '.')
            {
                tokenValue.Append(character);
                var token = OnNewToken(Token.Type.Separator);
                state = State.Field;
                return new[] { token };
            }
            else if (character == '}' && isInterpolation)
            {
                isInterpolation = false;
                state = State.String;
                return new[]
                {
                    new Token(")", Token.Type.Separator, currentPosition, currentLine),
                    new Token("+", Token.Type.Separator, currentPosition, currentLine),
                };
            }
            else if (Separators.Contains(character))
            {
                tokenValue.Append(character);
                return new[] { OnNewToken(Token.Type.Separator) };
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

        private IEnumerable<Token> Integer(char character)
        {
            if (character == '.')
            {
                state = State.Float;
                tokenValue.Append(character);
            }
            else if (char.IsWhiteSpace(character))
            {
                return new[] { OnNewToken(Token.Type.Integer) };
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(Token.Type.Integer, character);
            }
            else if (char.IsDigit(character))
            {
                tokenValue.Append(character);
            }
            else if (character == '_')
            {
            }
            else
            {
                throw new ArgumentException($"Integer number token contains unexpected character '{character}'");
            }

            return null;
        }

        private IEnumerable<Token> Float(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return new[] { OnNewToken(Token.Type.Float) };
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(Token.Type.Float, character);
            }
            else if (char.IsDigit(character))
            {
                tokenValue.Append(character);
            }
            else if (character == '_')
            {
            }
            else
            {
                throw new ArgumentException($"Float number token contains unexpected character '{character}'");
            }

            return null;
        }

        private IEnumerable<Token> String(char character)
        {
            if (character == '\\')
            {
                state = State.Slash;
            }
            else if (character == '\"')
            {
                return new[] { OnNewToken(Token.Type.String) };
            }
            else if (character == '{')
            {
                isInterpolation = true;
                state = State.None;
                return new[]
                {
                    OnNewToken(Token.Type.String),
                    new Token("+", Token.Type.Separator, currentPosition, currentLine),
                    new Token("(", Token.Type.Separator, currentPosition, currentLine),
                };
            }
            else
            {
                tokenValue.Append(character);
            }

            return null;
        }

        private IEnumerable<Token> Slash(char character)
        {
            if (character == 'n')
            {
                tokenValue.Append('\n');
            }
            else if (character == 't')
            {
                tokenValue.Append('\t');
            }
            else if (character == 'x')
            {
                state = State.HexSymbol;
                return null;
            }
            else
            {
                tokenValue.Append(character);
            }

            state = State.String;
            return null;
        }

        private IEnumerable<Token> HexSymbol(char character)
        {
            if (char.IsDigit(character) || "abcdefABCDEF".Contains(character))
            {
                hexSymbol += character;
            }
            else
            {
                throw new ArgumentException("Expected 2 digit hexadecimal number");
            }

            if (hexSymbol.Length < 2)
            {
                return null;
            }

            var hexChar = (char)(
                int.Parse(
                    hexSymbol,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture));

            tokenValue.Append(hexChar);
            hexSymbol = "";
            state = State.String;
            return null;
        }

        private IEnumerable<Token> Comment(char character)
        {
            if (character == '\n')
            {
                state = State.None;
            }

            return null;
        }

        private IEnumerable<Token> Identifier(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return new[] { OnNewToken(Token.Type.Identifier) };
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(Token.Type.Identifier, character);
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

        private IEnumerable<Token> RightAssign(char character)
        {
            if (character == '>')
            {
                tokenValue.Append(character);
                return new[] { OnNewToken(Token.Type.Separator) };
            }
            else
            {
                return OnExtraToken(Token.Type.Separator, character);
            }
        }

        private IEnumerable<Token> Apply(char character)
        {
            if (character == '>')
            {
                tokenValue.Append(character);
                return new[] { OnNewToken(Token.Type.Separator) };
            }
            else
            {
                return OnExtraToken(Token.Type.Separator, character);
            }
        }

        private IEnumerable<Token> Field(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return new[] { OnNewToken(Token.Type.String) };
            }
            else if (char.IsLetterOrDigit(character) || character == '_')
            {
                tokenValue.Append(character);
                return null;
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(Token.Type.String, character);
            }
            else
            {
                throw new ArgumentException($"Identifier contains unexpected character: '{character}'");
            }
        }

        /// <summary>
        /// Performs actions on a new token creating.
        /// </summary>
        /// <returns>A new created token.</returns>
        private Token OnNewToken(Token.Type tokenType)
        {
            state = State.None;
            var value = tokenValue.ToString();
            tokenValue.Clear();

            return new Token(value, tokenType, tokenStartPosition, currentLine);
        }

        /// <summary>
        /// Handles a situation when the given character bounds the previous token.
        /// </summary>
        /// <param name="prevTokenType">Previous token type.</param>
        /// <param name="character">Current character that bounds the previous token.</param>
        /// <returns>All created tokens.</returns>
        private IEnumerable<Token> OnExtraToken(Token.Type prevTokenType, char character)
        {
            var prevToken = OnNewToken(prevTokenType);
            var followingTokens = stateHandlers[state](character);
            return
                followingTokens == null
                ? new[] { prevToken }
                : followingTokens.Prepend(prevToken);
        }
    }
}