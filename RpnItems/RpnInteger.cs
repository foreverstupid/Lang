using System;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents an integer number.
    /// </summary>
    public class RpnInteger : RpnConst
    {
        private readonly int value;

        public RpnInteger(int val) => value = val;

        public RpnInteger(bool val) => value = val ? 1 : 0;

        public RpnInteger(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.Integer)
            {
                throw new ArgumentException("Token has not a type of an integer number");
            }

            if (int.TryParse(token.Value, out int value))
            {
                this.value = value;
                ValueType = Type.Integer;
            }
            else
            {
                throw new ArgumentException("Integer token has an invalid value: " + token.Value);
            }
        }

        /// <inheritdoc/>
        public override Type ValueType { get; }

        /// <inheritdoc/>
        public override double GetFloat() => (double)value;

        /// <inheritdoc/>
        public override int GetInt() => value;

        /// <inheritdoc/>
        public override string GetString() => value.ToString();

        /// <inheritdoc/>
        public override bool GetBool() => value != 0;
    }
}