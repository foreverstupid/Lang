using System;
using System.Collections.Generic;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that applies the given function to the given args.
    /// </summary>
    public class RpnEval : Rpn
    {
        private readonly Action<LinkedListNode<Rpn>> setReturnCommand;
        private readonly IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions;
        private readonly IReadOnlyDictionary<string, RpnConst> variables;
        private readonly int paramCount;

        public RpnEval(
            Token token,
            int paramCount,
            IReadOnlyDictionary<string, LinkedListNode<Rpn>> functions,
            IReadOnlyDictionary<string, RpnConst> variables,
            Action<LinkedListNode<Rpn>> setReturnCommand
        )
            : base(token)
        {
            this.paramCount = paramCount;
            this.functions = functions;
            this.variables = variables;
            this.setReturnCommand = setReturnCommand;
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

            var func = stack.Pop();
            if (func.ValueType == RpnConst.Type.BuiltIn)
            {
                if (BuiltIns.Funcs.TryGetValue(func.GetString(), out var builtIn))
                {
                    OnBuiltIn(stack, builtIn);
                    return currentCmd.Next;
                }
                else
                {
                    throw new InterpretationException(
                        $"Unknown built-in function '{func.GetString()}'"
                    );
                }
            }

            string funcName;
            if (func.ValueType == RpnConst.Type.Func)
            {
                funcName = func.GetString();
            }
            else if (func.ValueType == RpnConst.Type.Variable)
            {
                if (variables.TryGetValue(func.GetString(), out var value))
                {
                    funcName = value.GetString();
                }
                else
                {
                    throw new InterpretationException($"Unknown variable {func.GetString()}");
                }
            }
            else
            {
                throw new InterpretationException("Cannot evaluate not function");
            }

            if (functions.TryGetValue(funcName, out var funcStart))
            {
                OnFunc(stack);
                return funcStart;
            }
            else
            {
                throw new InterpretationException(
                    "Unknown function"
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

            void OnFunc(Stack<RpnConst> stack)
            {
                for (int i = parameters.Length - 1; i >= 0; i--)
                {
                    stack.Push(parameters[i]);
                }

                setReturnCommand(currentCmd.Next);
            }
        }
    }
}