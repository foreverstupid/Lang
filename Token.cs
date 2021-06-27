namespace Lang
{
    /// <summary>
    /// Unit of the meaningful information.
    /// </summary>
    public class Token
    {
        public Token(string value, Token.Type type, int startPosition, int line)
        {
            Value = value;
            TokenType = type;
            StartPosition = startPosition;
            Line = line;
        }

        public enum Type
        {
            Identifier,
            String,
            Integer,
            Float,
            Separator,
        }

        public string Value { get; }

        public Type TokenType { get; }

        public int Line { get; }

        public int StartPosition { get; }

        /// <inheritdoc/>
        public override string ToString()
            => $"[{TokenType}] {Value} ({Line}:{StartPosition})";
    }
}