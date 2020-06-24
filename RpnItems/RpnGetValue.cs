namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that represents dereference (getting value of the variable).
    /// </summary>
    public class RpnGetValue : RpnUnaryOperation
    {
        public RpnGetValue(Token token)
            : base(token)
        {
        }

        /// <inheritdoc/>
        protected override int Priority => RpnOperation.DereferencePriority;

        /// <inheritdoc/>
        protected override RpnConst GetResultCore(RpnConst operand)
        {
            // TODO: positional args
            if (operand.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot get a value of a non-variable entity");
            }

            var variable = operand as RpnVar;
            try
            {
                return variable.VariableValue.Get();
            }
            catch (GetVariableValueException)
            {
                throw new InterpretationException($"The variable {this.Token.Value} doesn't exist");
            }
        }
    }
}