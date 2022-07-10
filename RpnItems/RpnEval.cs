using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that applies the given function to the given args.
    /// </summary>
    public sealed class RpnEval : Rpn
    {
        private readonly int paramCount;
        private readonly RpnLabel returnLabel;
        private readonly IReadOnlyDictionary<string, BuiltInLibrary.Func> builtIns;
        private readonly IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions;
        private readonly IReadOnlyDictionary<string, RpnConst> variables;

        public RpnEval(
            Token token,
            int paramCount,
            RpnLabel returnLabel,
            IReadOnlyDictionary<string, BuiltInLibrary.Func> builtIns,
            IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions,
            IReadOnlyDictionary<string, RpnConst> variables
        )
            : base(token)
        {
            this.paramCount = paramCount;
            this.returnLabel = returnLabel;
            this.builtIns = builtIns;
            this.functions = functions;
            this.variables = variables;
        }

        /// <inheritdoc/>
        public override LinkedListNode<Rpn> Eval(
            Stack<RpnConst> stack,
            LinkedListNode<Rpn> currentCmd)
        {
            var parameters = new RpnConst[paramCount];
            for (int i = paramCount - 1; i >= 0; i--)
            {
                parameters[i] = stack.Pop();
            }

            var executingEntity = stack.Pop();

            // extract variable value to make user's life simpler
            if (executingEntity.ValueType == RpnConst.Type.Variable)
            {
                if (!variables.TryGetValue(executingEntity.GetString(), out executingEntity))
                {
                    throw new InterpretationException($"Unknown variable {executingEntity.GetString()}");
                }
            }

            if (executingEntity.ValueType == RpnConst.Type.BuiltIn)
            {
                OnBuiltIn(stack, executingEntity.GetString(), parameters);
                return currentCmd.Next;
            }
            else if (executingEntity.ValueType == RpnConst.Type.Func)
            {
                if (!functions.TryGetValue(executingEntity.GetString(), out var funcStart))
                {
                    throw new InterpretationException("Unknown function to evaluate");
                }

                stack.Push(returnLabel);
                for (int i = parameters.Length - 1; i >= 0; i--)
                {
                    stack.Push(parameters[i]);
                }

                return funcStart;
            }
            else
            {
                throw new InterpretationException(
                    "Only functions can be evaluated");
            }
        }

        private void OnBuiltIn(
            Stack<RpnConst> stack,
            string builtInName,
            RpnConst[] parameters)
        {
            if (!builtIns.TryGetValue(builtInName, out var builtIn))
            {
                // this shouldn't happen, but who knows...
                throw new InterpretationException(
                    $"Unknown built-in function {builtInName}");
            }

            if (paramCount != builtIn.ParamCount)
            {
                throw new InterpretationException(
                    $"Buil-in fucntion '{builtInName}' should have " +
                    $"{builtIn.ParamCount} parameters");
            }

            var result = builtIn.Main(parameters);
            stack.Push(result);
        }
    }
}