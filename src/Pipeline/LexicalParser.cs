using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lang.Exceptions;
using Lang.Content;

namespace Lang.Pipeline
{
    /// <summary>
    /// Performs lexical analysis of the source code.
    /// </summary>
    public sealed class LexicalParser
    {
        private const char StandardInterpolationStart = '{';
        private const char StandardInterpolationEnd = '}';
        private const char StandardStringEnd = '\"';

        private static readonly string Separators = ";,=$.+-*/%><!|&~()[]{}:?";
        private static readonly string[] TwoCharsOperations = new[]
        {
            Operations.Insert,
            Operations.RightShift,
            Operations.LeftShift,
            Syntax.ParamsApply,
        };

        private readonly Dictionary<State, Func<char, IEnumerable<Token>>> stateHandlers;

        private readonly StringBuilder tokenValue = new StringBuilder();
        private int tokenStartPosition;
        private int currentPosition = 1;
        private int currentLine = 1;
        private State state = State.None;

        private string hexSymbol = "";
        private bool isInterpolation = false;

        private char stringEnd = StandardStringEnd;
        private char interpolationStart = StandardInterpolationStart;
        private char interpolationEnd = StandardInterpolationEnd;

        private bool isRawString = false;
        private int rawStringShift = 0;
        private int skippedSpaces = 0;
        private bool interpolationStartSet = false;

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
                [State.RawStringPreambula] = RawStringPreambula,
                [State.SkipRawShift] = SkipRawShift,
                [State.Slash] = Slash,
                [State.HexSymbol] = HexSymbol,
                [State.Comment] = Comment,
                [State.Field] = Field,
                [State.TwoCharsOperation] = TwoCharsOperation,
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
            RawStringPreambula,
            SkipRawShift,
            Slash,
            HexSymbol,
            Comment,
            Identifier,
            Field,
            TwoCharsOperation
        }

        /// <summary>
        /// Parses a given source code into the collection of sequential tokens.
        /// </summary>
        /// <param name="src">The program source code.</param>
        /// <returns>The tokens that was generated from the given source code.</returns>
        public IEnumerable<Token> Parse(string src)
        {
            try
            {
                return ParseCore(src);
            }
            catch (Exception e)
            {
                throw new LexicalAnalysisException(
                    $"({currentLine}:{currentPosition}) {e.Message}");
            }
        }

        public IEnumerable<Token> ParseCore(string src)
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
            else if (character == '`')
            {
                state = State.RawStringPreambula;
                rawStringShift = currentPosition;
                stringEnd = '`';
                isRawString = true;
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
            else if (TwoCharsOperations.Any(op => op[0] == character))
            {
                state = State.TwoCharsOperation;
                tokenValue.Append(character);
            }
            else if (character == '.')
            {
                tokenValue.Append(character);
                var token = OnNewToken(Token.Type.Separator);
                state = State.Field;
                return new[] { token };
            }
            else if (character == interpolationEnd && isInterpolation)
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
            if (isRawString && character == '\n')
            {
                state = State.SkipRawShift;
                skippedSpaces = 0;
                tokenValue.Append(character);
                return null;
            }

            if (character == '\\')
            {
                state = State.Slash;
            }
            else if (character == stringEnd)
            {
                if (isRawString)
                {
                    isRawString = false;
                    interpolationStart = StandardInterpolationStart;
                    interpolationEnd = StandardInterpolationEnd;
                    stringEnd = StandardStringEnd;
                }

                return new[] { OnNewToken(Token.Type.String) };
            }
            else if (character == interpolationStart)
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

        private IEnumerable<Token> RawStringPreambula(char character)
        {
            if (character == '`' || character == '#')
            {
                throw new ArgumentException(
                    "Characters ` and # cannot be used as interpolation delimiters");
            }

            if (interpolationStartSet)
            {
                interpolationStartSet = false;
                interpolationEnd = character;
                state = State.String;
            }
            else
            {
                interpolationStart = character;
                interpolationStartSet = true;
            }

            return null;
        }

        private IEnumerable<Token> SkipRawShift(char character)
        {
            if (character != ' ' || skippedSpaces >= rawStringShift)
            {
                state = State.String;
                return String(character);
            }

            skippedSpaces++;
            return null;
        }

        private IEnumerable<Token> Slash(char character)
        {
            if (character == 'x')
            {
                state = State.HexSymbol;
                return null;
            }

            state = State.String;

            if (isRawString)
            {
                if (character == '`')
                {
                    tokenValue.Append(character);
                    return null;
                }
                else
                {
                    tokenValue.Append('\\');    // push missed raw slash
                    return String(character);
                }
            }

            if (character == 'n')
            {
                tokenValue.Append('\n');
            }
            else if (character == 't')
            {
                tokenValue.Append('\t');
            }
            else
            {
                tokenValue.Append(character);
            }

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
                return new[] { OnNewToken(GetTokenType()) };
            }
            else if (Separators.Contains(character))
            {
                return OnExtraToken(GetTokenType(), character);
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

            Token.Type GetTokenType()
            {
                var value = tokenValue.ToString();
                if (KeyWords.All.Contains(value))
                {
                    return Token.Type.Separator;
                }

                return Token.Type.Identifier;
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

        private IEnumerable<Token> TwoCharsOperation(char character)
        {
            var operation = TwoCharsOperations
                .FirstOrDefault(op => op[0] == tokenValue[0] && op[1] == character);

            if (operation != null)
            {
                tokenValue.Append(character);
                return new[] { OnNewToken(Token.Type.Separator) };
            }
            else
            {
                return OnExtraToken(Token.Type.Separator, character);
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