using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that applies the given function to the given args.
    /// </summary>
    public class RpnEval : RpnSuccessive
    {
        private readonly IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions;
        private readonly int paramCount;

        public RpnEval(
            Token token,
            int paramCount,
            IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions
        )
            : base(token)
        {
            this.functions = functions;
            this.paramCount = paramCount;
        }

        /// <inheritdoc/>
        protected override void EvalCore(Stack<RpnConst> stack)
        {
            var parameters = new RpnConst[paramCount];
            for (int i = paramCount - 1; i >= 0; i--)
            {
                parameters[i] = stack.Pop();
            }

            var func = stack.Pop();
            if (func.ValueType == RpnConst.Type.BuiltIn)
            {
                if (BuiltIns.Funcs.TryGetValue(func.GetString(), out var builtIn))
                {
                    OnBuiltIn(stack, builtIn);
                    return;
                }
                else
                {
                    throw new InterpretationException(
                        $"Unknown built-in function '{func.GetString()}'"
                    );
                }
            }
            
            if (func.ValueType != RpnConst.Type.Variable)
            {
                throw new InterpretationException("Cannot evaluate non-variable value");
            }

            if (functions.TryGetValue(func.GetString(), out var funcStart))
            {
                //OnFunc(stack, funcStart);
            }
            else
            {
                throw new InterpretationException(
                    "Cannot evaluate variable that is not a function"
                );
            }

            void OnBuiltIn(Stack<RpnConst> stack, BuiltIns.BuiltinFunc builtIn)
            {
                if (paramCount != builtIn.ParamCount)
                {
                    throw new InterpretationException(
                        $"Buil-in fucntion '{func.GetString()}' should have " +
                        $"{builtIn.ParamCount} parameters"
                    );
                }

                var result = builtIn.Func(parameters);
                stack.Push(result);
            }
        }
    }
}