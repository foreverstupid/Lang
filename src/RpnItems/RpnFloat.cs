using System;
using System.Globalization;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a float point number.
    /// </summary>
    public sealed class RpnFloat : RpnConst
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

            if (!double.TryParse(
                    token.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new ArgumentException("Float token has an invalid value: " + token.Value);
            }
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.Float;

        /// <inheritdoc/>
        public override double GetFloat() => value;

        /// <inheritdoc/>
        public override int GetInt() => (int)value;

        /// <inheritdoc/>
        public override string GetString() => value.ToString();

        /// <inheritdoc/>
        public override bool GetBool() => value != 0.0;

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnFloat) + ": " + value;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetFloat() == this.GetFloat();
    }
}