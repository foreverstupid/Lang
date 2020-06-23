using System;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a float point number.
    /// </summary>
    public class RpnFloat : RpnConst
    {
        private readonly double value;

        public RpnFloat(double val) => value = val;

        public RpnFloat(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.Float)
            {
                throw new ArgumentException("Token has not a type of a float number");
            }

            if (double.TryParse(token.Value, out double value))
            {
                this.value = value;
                ValueType = Type.Float;
            }
            else
            {
                throw new ArgumentException("Float token has an invalid value: " + token.Value);
            }
        }

        /// <inheritdoc/>
        public override Type ValueType { get; }

        /// <inheritdoc/>
        public override double GetFloat() => value;

        /// <inheritdoc/>
        public override int GetInt() => (int)value;

        /// <inheritdoc/>
        public override string GetString() => value.ToString();
    }
}