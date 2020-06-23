using System;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an integer number.
    /// </summary>
    public class RpnInteger : RpnConst
    {
        public RpnInteger(int val) => Value = val;

        public RpnInteger(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.Integer)
            {
                throw new ArgumentException("Token has not a type of an integer number");
            }

            if (int.TryParse(token.Value, out int value))
            {
                Value = value;
            }
            else
            {
                throw new ArgumentException("Integer token has an invalid value: " + token.Value);
            }
        }

        /// <inheritdoc/>
        public override object Value { get; }
    }
}