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
        }

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

        /// <inheritdoc/>
        protected override void EvalCore(Stack<RpnConst> stack)
            => stack.Push(this);
    }
}