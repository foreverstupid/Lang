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

        public override string ToString()
            => this.GetType().Name + (Token is null ? "" : $": {Token.Value}");
    }
}