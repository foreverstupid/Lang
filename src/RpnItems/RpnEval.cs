using System.Collections.Generic;
using Lang.Content;
using Lang.Exceptions;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that applies the given function to the given args.
    /// </summary>
    public sealed class RpnEval : Rpn
    {
        private readonly int paramCount;
        private readonly RpnLabel returnLabel;
        private readonly IReadOnlyDictionary<EntityName, BuiltInLibrary.Func> builtIns;
        private readonly IReadOnlyDictionary<EntityName, LinkedListNode<Rpn>> functions;
        private readonly IReadOnlyDictionary<EntityName, RpnConst> variables;

        public RpnEval(
            Token token,
            int paramCount,
            RpnLabel returnLabel,
            IReadOnlyDictionary<EntityName, BuiltInLibrary.Func> builtIns,
            IReadOnlyDictionary<EntityName, LinkedListNode<Rpn>> functions,
            IReadOnlyDictionary<EntityName, RpnConst> variables
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
                if (!variables.TryGetValue(executingEntity.GetName(), out executingEntity))
                {
                    throw new InterpretationException($"Unknown variable for evaluation");
                }
            }

            if (executingEntity.ValueType == RpnConst.Type.BuiltIn)
            {
                OnBuiltIn(stack, executingEntity.GetName(), parameters);
                return currentCmd.Next;
            }
            else if (executingEntity.ValueType == RpnConst.Type.Func)
            {
                if (!functions.TryGetValue(executingEntity.GetName(), out var funcStart))
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
            EntityName builtInName,
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

            for (int i = 0; i < paramCount; i++)
            {
                if (!builtIn.ParamTypes[i].HasFlag(parameters[i].ValueType))
                {
                    throw new InterpretationException(
                        $"Wrong type of the {i + 1}-th parameter of built-in function " +
                        $"'{builtInName}'. It should be: {builtIn.ParamTypes[i]}");
                }
            }

            var result = builtIn.Main.Invoke(parameters);
            stack.Push(result);
        }
    }
}