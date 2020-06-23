using System;

namespace Lang.RpnItems
{
    public class RpnString : RpnConst
    {
        public RpnString(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.String)
            {
                throw new ArgumentException("Token has not a type of a string");
            }

            Value = token.Value;
        }

        public string Value { get; }
    }
}