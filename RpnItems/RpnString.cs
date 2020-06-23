using System;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a string.
    /// </summary>
    public class RpnString : RpnConst
    {
        public RpnString(string val) => Value = val;

        public RpnString(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.String)
            {
                throw new ArgumentException("Token has not a type of a string");
            }

            Value = token.Value;
        }

        /// <inheritdoc/>
        public override object Value { get; }
    }
}