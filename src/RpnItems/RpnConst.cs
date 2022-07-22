using System;
using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents some value.
    /// </summary>
    public abstract class RpnConst : RpnSuccessive
    {
        public RpnConst()
            : base(null)
        {
        }

        public RpnConst(Token token)
            : base(token)
        {
        }

        /// <summary>
        /// The value type.
        /// </summary>
        public enum Type
        {
            Integer,
            Float,
            String,
            Label,
            Variable,
            BuiltIn,
            Func,
            None,
        }

        /// <summary>
        /// Value representing true.
        /// </summary>
        public static RpnConst True { get; } = new RpnInteger(1);

        /// <summary>
        /// Value representing false.
        /// </summary>
        /// <returns></returns>
        public static RpnConst False { get; } = new RpnInteger(0);

        /// <summary>
        /// Value representing None.
        /// </summary>
        public static RpnConst None { get; } = new RpnNone();

        /// <summary>
        /// The type of the item value.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// The value of the item as integer.
        /// </summary>
        /// <remarks>Throws if the cast is not possible.</remarks>
        public abstract int GetInt();

        /// <summary>
        /// The value of the item as float.
        /// </summary>
        /// <remarks>Throws if the cast is not possible.</remarks>
        public abstract double GetFloat();

        /// <summary>
        /// The value of the item as integer.
        /// </summary>
        /// <remarks>Throws if the cast is not possible.</remarks>
        public abstract string GetString();

        /// <summary>
        /// The value of the item as bool.
        /// </summary>
        /// <remarks>Throws if the cast is not possible.</remarks>
        public abstract bool GetBool();

        /// <summary>
        /// The name of the item. It is allowed only for named constants
        /// (e.g. variables).
        /// </summary>
        /// <remarks>Throws for unnamed constants.</remarks>
        public virtual EntityName GetName()
        {
            throw new Exception($"Constant of type '{ValueType}' has no name");
        }

        /// <summary>
        /// Converts boolean value to pseudo-bool RPN representation.
        /// </summary>
        /// <param name="flag">Converting boolean value.</param>
        /// <returns>Pseudo-bool RPN representation.</returns>
        public static RpnConst Bool(bool flag)
            => flag ? True : False;

        /// <inheritdoc/>
        protected override sealed void EvalCore(Stack<RpnConst> stack)
            => stack.Push(this);

        /// <inheritdoc/>
        public abstract override string ToString();

        /// <summary>
        /// Checks whether the constant has the smae value as the given one.
        /// </summary>
        /// <param name="other">The given constant.</param>
        /// <returns>A boolean value, that indicates, whether the constant
        /// contains the same value as the given one.</returns>
        public bool HasSameValue(RpnConst other)
            => other != null &&
               other.ValueType == this.ValueType &&
               HasSameValueCore(other);

        /// <summary>
        /// Runs the core logic of the constant value comparision.
        /// It is guaranteed, that <paramref name="other"/> is not
        /// <see langword="null"/> and has the same type as the current
        /// constant.
        /// </summary>
        /// <param name="other">The given constant.</param>
        /// <returns>A boolean value, that indicates, whether the constant
        /// contains the same value as the given one.</returns>
        protected abstract bool HasSameValueCore(RpnConst other);
    }
}