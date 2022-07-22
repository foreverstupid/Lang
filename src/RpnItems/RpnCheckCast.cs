using System;
using System.Collections.Generic;
using System.Globalization;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    using CastCheckMap = Dictionary<(RpnConst.Type, RpnConst.Type), Func<RpnConst, bool>>;

    /// <summary>
    /// RPN item that checks whether casting of the left operand to the type of the
    /// right one is possible.
    /// </summary>
    public sealed class RpnCheckCast : RpnBinaryOperation
    {
        private static readonly CastCheckMap CastMap = new CastCheckMap()
        {
            [(RpnConst.Type.String, RpnConst.Type.Integer)] = str =>
                int.TryParse(
                    str.GetString(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out _),
            [(RpnConst.Type.String, RpnConst.Type.Float)] = str =>
                double.TryParse(
                    str.GetString(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out _),
            [(RpnConst.Type.String, RpnConst.Type.String)] = _ => true,
            [(RpnConst.Type.Integer, RpnConst.Type.Integer)] = _ => true,
            [(RpnConst.Type.Integer, RpnConst.Type.Float)] = _ => true,
            [(RpnConst.Type.Integer, RpnConst.Type.String)] = _ => true,
            [(RpnConst.Type.Float, RpnConst.Type.Integer)] = _ => true,
            [(RpnConst.Type.Float, RpnConst.Type.Float)] = _ => true,
            [(RpnConst.Type.Float, RpnConst.Type.String)] = _ => true,
        };

        public RpnCheckCast(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.CastPriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst left, RpnConst right)
        {
            bool canCast =
                CastMap.TryGetValue((left.ValueType, right.ValueType), out var checker) &&
                checker(left);

            return RpnConst.Bool(canCast);
        }
    }
}