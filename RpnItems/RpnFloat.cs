using System;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a float point number.
    /// </summary>
    public class RpnFloat : RpnConst
    {
        public RpnFloat(double val) => this.Value = val;

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

        /// <inheritdoc/>
        public override object Value { get; }
    }
}