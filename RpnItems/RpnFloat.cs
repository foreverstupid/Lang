using System;

namespace Lang.RpnItems
{
    public class RpnFloat : RpnConst
    {
        public RpnFloat(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.Float)
            {
                throw new ArgumentException("Token has not a type of a float number");
            }

            if (double.TryParse(token.Value, out double value))
            {
                Value = value;
            }
            else
            {
                throw new ArgumentException("Float token has an invalid value: " + token.Value);
            }
        }

        public double Value { get; }
    }
}