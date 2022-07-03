using System;
using System.Globalization;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an integer number.
    /// </summary>
    public sealed class RpnInteger : RpnConst
    {
        private readonly int value;

        public RpnInteger(int val) => value = val;

        public RpnInteger(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.Integer)
            {
                throw new ArgumentException("Token has not a type of an integer number");
            }

            if (!int.TryParse(
                    token.Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new ArgumentException("Integer token has an invalid value: " + token.Value);
            }
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.Integer;

        /// <inheritdoc/>
        public override double GetFloat() => (double)value;

        /// <inheritdoc/>
        public override int GetInt() => value;

        /// <inheritdoc/>
        public override string GetString() => value.ToString();

        /// <inheritdoc/>
        public override bool GetBool() => value != 0;

        /// <inheritdoc/>
        public override string ToString()
            => nameof(RpnInteger) + ": " + value;

        /// <inheritdoc/>
        protected override bool HasSameValueCore(RpnConst other)
            => other.GetInt() == this.GetInt();
    }
}