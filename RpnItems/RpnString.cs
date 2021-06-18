using System;
using System.Globalization;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents a string.
    /// </summary>
    public class RpnString : RpnConst
    {
        public readonly string value;

        public RpnString(string val) => value = val;

        public RpnString(Token token)
            : base(token)
        {
            if (token.TokenType != Token.Type.String)
            {
                throw new ArgumentException("Token has not a type of a string");
            }

            this.value = token.Value;
        }

        /// <inheritdoc/>
        public override Type ValueType => Type.String;

        /// <inheritdoc/>
        public override double GetFloat() =>
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var res)
            ? res
            : throw new InterpretationException("Cannot convert string into a float");

        /// <inheritdoc/>
        public override int GetInt() =>
            int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var res)
            ? res
            : throw new InterpretationException("Cannot convert string into an integer");

        /// <inheritdoc/>
        public override string GetString() => value;

        /// <inheritdoc/>
        public override bool GetBool() => value != "";

        /// <inheritdoc/>
        public override string ToString()
            => this.GetType().Name + ": " + value;
    }
}