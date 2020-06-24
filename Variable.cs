using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// The class that represents a variable value.
    /// </summary>
    public class Variable
    {
        private bool isInitialized = false;
        private RpnConst value;

        /// <summary>
        /// Sets the variable with the given value.
        /// </summary>
        public void Set(RpnConst value)
        {
            this.value = value;
            isInitialized = true;
        }

        /// <summary>
        /// Gets the value of the variable.
        /// </summary>
        /// <remarks>Throws if the variable is not initialized.</remarks>
        public RpnConst Get()
        {
            if (!isInitialized)
            {
                throw new GetVariableValueException();
            }

            return value;
        }
    }
}