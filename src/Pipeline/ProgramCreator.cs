using System.Collections.Generic;
using System.Linq;
using Lang.RpnItems;
using Lang.Exceptions;
using Lang.Content;

namespace Lang.Pipeline
{
    /// <summary>
    /// Creates the needed program interpretation info.
    /// </summary>
    public class ProgramCreator
    {
        private const string IfLabelPrefix = "<if>";
        private const string ElseLabelPrefix = "<or>";
        private const string LambdaPrefix = "<f";
        private const string LambdaEndPrefix = "</f";
        private const string ReturnLabelPrefix = "<ret";

        // help variables
        private readonly List<EntityName> labelsForNextRpn = new List<EntityName>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();
        private readonly Stack<int> ifIdxStack = new Stack<int>();
        private readonly Stack<int?> loopIdxStack = new Stack<int?>();
        private readonly Stack<LambdaContext> contextsStack = new Stack<LambdaContext>();

        private int ifCount = 0;
        private int lambdaCount = 0;
        private int returnCount = 0;

        // creating program information
        private LinkedList<Rpn> program;
        private Dictionary<EntityName, RpnConst> variables;
        private Dictionary<EntityName, LinkedListNode<Rpn>> labels;
        private Dictionary<EntityName, LinkedListNode<Rpn>> lambdas;
        private List<string> globalRefVars;
        private BuiltInLibrary funcLibrary;

        /// <summary>
        /// Starts a new program creation.
        /// </summary>
        public void StartProgramCreation()
        {
            program = new LinkedList<Rpn>();
            variables = new Dictionary<EntityName, RpnConst>();
            labels = new Dictionary<EntityName, LinkedListNode<Rpn>>();
            lambdas = new Dictionary<EntityName, LinkedListNode<Rpn>>();
            globalRefVars = new List<string>();
            funcLibrary = new BuiltInLibrary(variables);

            labelsForNextRpn.Clear();
            expressionStack.Clear();
            ifIdxStack.Clear();
            loopIdxStack.Clear();
            contextsStack.Clear();

            ifCount = lambdaCount = returnCount = 0;

            OpenBracket();
        }

        /// <summary>
        /// Finishes the program creation.
        /// </summary>
        /// <returns>The program RPN representation.</returns>
        public LinkedList<Rpn> FinishProgramCreation()
        {
            CloseBracket();

            if (contextsStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, lambda context stack is not empty"
                );
            }

            if (ifIdxStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, 'if' stack is not empty"
                );
            }

            labelsForNextRpn.Clear();
            return program;
        }

        /// <summary>
        /// Creates an indexator.
        /// </summary>
        public void Indexator(Token token)
        {
            NewOperation(new RpnIndexator(token, variables));
        }

        /// <summary>
        /// Creates a new program entity that is represented as literal.
        /// </summary>
        public void Literal(Token token)
        {
            RpnConst rpn;

            if (token.TokenType == Token.Type.Identifier)
            {
                foreach (var ctx in contextsStack.Reverse())
                {
                    if (ctx.HasLocalVariable(token.Value))
                    {
                        var name = ctx.LambdaName + token.Value;
                        if (ctx.IsVariableRef(token.Value))
                        {
                            OpenBracket();
                            AddRpn(new RpnVar(token, name, variables));
                            AddRpn(new RpnGetValue(token, variables));
                            CloseBracket();
                            return;
                        }

                        AddRpn(new RpnVar(token, name, variables));
                        return;
                    }
                }

                if (globalRefVars.Contains(token.Value))
                {
                    OpenBracket();
                    AddRpn(new RpnVar(token, token.Value, variables));
                    AddRpn(new RpnGetValue(token, variables));
                    CloseBracket();
                    return;
                }

                rpn = new RpnVar(token, token.Value, variables);
            }
            else
            {
                rpn = token.TokenType switch
                {
                    Token.Type.Float => new RpnFloat(token),
                    Token.Type.Integer => new RpnInteger(token),
                    Token.Type.String => new RpnString(token),
                    _ => throw new RpnCreationException(
                        $"Unexpected literal token type {token.TokenType}"
                    )
                };
            }

            AddRpn(rpn);
        }

        /// <summary>
        /// Adds a new unary operation.
        /// </summary>
        public void UnaryOperation(Token token)
        {
            RpnOperation rpn = token.Value switch
            {
                Operations.Not => new RpnNot(token),
                Operations.Minus => new RpnNegate(token),
                Operations.Dereference => new RpnGetValue(token, variables),
                var op => throw new RpnCreationException("Unknown unary operation: " + op)
            };

            NewOperation(rpn);
        }

        /// <summary>
        /// Adds a new binary operation.
        /// </summary>
        public void BinaryOperation(Token token, bool isReversed = false)
        {
            var rpn = GetBinaryOperation(token, isReversed);
            NewOperation(rpn);
        }

        /// <summary>
        /// Adds an assignment.
        /// </summary>
        public void Assignment(Token token, Token additional = null, bool isReversed = false)
        {
            var additionalOperation =
                additional is null
                ? null
                : GetBinaryOperation(additional, isReversed);

            RpnOperation rpn = token.Value switch
            {
                Operations.Assign => new RpnAssign(token, variables, additionalOperation),
                Operations.Insert => new RpnInsert(token, variables, additionalOperation),
                var op => throw new RpnCreationException("Unknown assignment operation: " + op)
            };

            NewOperation(rpn);
        }

        /// <summary>
        /// Starts creating of a lambda.
        /// </summary>
        public void LambdaStart()
        {
            var context = new LambdaContext(lambdaCount);
            lambdaCount++;
            contextsStack.Push(context);
            loopIdxStack.Push(null);
            OpenBracket();

            // add jump to the end of the code block
            Label(context.EndLabel);
            AddRpn(new RpnGoto(labels));

            AddRpn(new RpnNop());
            lambdas.Add(new EntityName(context.LambdaName), program.Last);
        }

        /// <summary>
        /// Finishes creating of a lambda
        /// </summary>
        public void LambdaFinish()
        {
            var context = contextsStack.Pop();
            loopIdxStack.Pop();
            CloseBracket();

            AddRpn(new RpnReturn(labels));
            AddLabelForNextRpn(context.EndLabel);
            AddRpn(new RpnFunc(context.LambdaName));
        }

        /// <summary>
        /// Creates an initializer of a lambda parameter.
        /// </summary>
        public void Parameter(Token token, bool isRef = false)
        {
            LocalVariable(token, isRef);
            AddRpn(new RpnInsert(token, variables));
            AddRpn(new RpnIgnore());
        }

        /// <summary>
        /// Adds a new local variable.
        /// </summary>
        public void LocalVariable(Token token, bool isRef = false)
        {
            string prefix = "";
            if (contextsStack.TryPeek(out var ctx))
            {
                ctx.AddLocalVariable(token.Value, isRef);
                prefix = ctx.LambdaName;
            }

            AddRpn(new RpnVar(token, prefix + token.Value, variables));
        }

        /// <summary>
        /// Adds a new global ref variable.
        /// </summary>
        public void GlobalRefVariable(Token token)
        {
            globalRefVars.Add(token.Value);
            AddRpn(new RpnVar(token, token.Value, variables));
        }

        /// <summary>
        /// Adds expression value ignoring RPN.
        /// </summary>
        public void Ignore(Token token)
        {
            AddRpn(new RpnIgnore(token));
        }

        /// <summary>
        /// Starts creation og if-expression.
        /// </summary>
        public void IfStart()
        {
            AddRpn(RpnConst.None);
        }

        /// <summary>
        /// Creates condition checking part of if-expression.
        /// </summary>
        public void If(Token token)
        {
            ifCount++;
            ifIdxStack.Push(ifCount);
            var ifLabelName = IfLabelPrefix + ifCount;

            Label(ifLabelName);
            AddRpn(new RpnIfGoto(token, labels));
            AddRpn(new RpnIgnore());    // ignore the None value if the condition is true
        }

        /// <summary>
        /// Adds else-part to the if-expression.
        /// </summary>
        public void Else(Token token)
        {
            var idx = ifIdxStack.Peek();
            var elseLabelName = ElseLabelPrefix + idx;
            var ifLabelName = IfLabelPrefix + idx;

            Label(elseLabelName);
            AddRpn(new RpnGoto(labels));

            AddLabelForNextRpn(ifLabelName);
            AddRpn(new RpnIgnore());    // ignore the None value if have else-part
        }

        /// <summary>
        /// Finishes creating if-only-expression.
        /// </summary>
        public void EndIf()
        {
            var idx = ifIdxStack.Pop();
            var ifLabelName = IfLabelPrefix + idx;
            AddLabelForNextRpn(ifLabelName);
            AddRpn(new RpnNop());
        }

        /// <summary>
        /// Finishes creating if-else-expression.
        /// </summary>
        public void EndElse()
        {
            var idx = ifIdxStack.Pop();
            var elseLabelName = ElseLabelPrefix + idx;
            AddLabelForNextRpn(elseLabelName);
            AddRpn(new RpnNop());
        }

        /// <summary>
        /// Starts creating of the cycle.
        /// </summary>
        public void CycleStart()
        {
            ifCount++;
            loopIdxStack.Push(ifCount);

            var cycleStartLabel = IfLabelPrefix + ifCount;

            AddRpn(RpnConst.None);
            AddLabelForNextRpn(cycleStartLabel);
        }

        /// <summary>
        /// Creates condition checking part of the cycle.
        /// </summary>
        public void While(Token token)
        {
            var idx = loopIdxStack.Peek();
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleEndLabelName);
            AddRpn(new RpnIfGoto(token, labels));
            AddRpn(new RpnIgnore());    // ignores the value of the last iteration
        }

        /// <summary>
        /// Finishes creating of the cycle.
        /// </summary>
        public void CycleEnd()
        {
            var idx = loopIdxStack.Pop();
            var cycleStartLabelName = IfLabelPrefix + idx;
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleStartLabelName);
            AddRpn(new RpnGoto(labels));

            AddLabelForNextRpn(cycleEndLabelName);
            AddRpn(new RpnNop());
        }

        /// <summary>
        /// Creates cycle break jump.
        /// </summary>
        public void CycleBreak()
        {
            if (!loopIdxStack.TryPeek(out var idx) || idx is null)
            {
                throw new RpnCreationException(
                    "Cycle break can be used only in a cycle expression");
            }

            AddRpn(RpnConst.True);
            Label(ElseLabelPrefix + idx.Value);
            AddRpn(new RpnGoto(labels));
        }

        /// <summary>
        /// Creates cycle continue jump.
        /// </summary>
        public void CycleContinue()
        {
            if (!loopIdxStack.TryPeek(out var idx) || idx is null)
            {
                throw new RpnCreationException(
                    "Cycle continue can be used only in a cycle expression");
            }

            AddRpn(RpnConst.True);
            Label(IfLabelPrefix + idx.Value);
            AddRpn(new RpnGoto(labels));
        }

        public void LambdaReturn()
        {
            if (!contextsStack.TryPeek(out _))
            {
                throw new RpnCreationException(
                    "Lambda break jump can be used only inside a lambda");
            }

            // hack to use prioritization for evaluate. TODO: make something reasonable
            FlushOperationsWithHigherOrEqualPriority(new RpnInsert(null, variables));
            AddRpn(new RpnReturn(labels));
        }

        /// <summary>
        /// Starts creation of evaluation.
        /// </summary>
        public void EvalStart()
        {
            // hack to use prioritization for evaluate. TODO: make something reasonable
            FlushOperationsWithHigherOrEqualPriority(new RpnGetValue(null, variables));
        }

        /// <summary>
        /// Adds evaluation token for the function of the certain amount of parameters.
        /// </summary>
        public void Eval(Token token, int paramCount)
        {
            string retLabelName = $"{ReturnLabelPrefix}{returnCount}>";
            var returnLabel = new RpnLabel(retLabelName);

            AddRpn(new RpnEval(
                token,
                paramCount,
                returnLabel,
                funcLibrary.Functions,
                lambdas,
                variables));

            AddLabelForNextRpn(retLabelName);
            AddRpn(new RpnNop());
            returnCount++;
        }

        /// <summary>
        /// Starts expression operations group.
        /// </summary>
        public void OpenBracket()
        {
            expressionStack.Push(null);
        }

        /// <summary>
        /// Finishes the expression operation group.
        /// </summary>
        public void CloseBracket()
        {
            while (expressionStack.TryPop(out var operation) && !(operation is null))
            {
                AddRpn(operation);
            }
        }

        private void LabelLastRpn()
        {
            foreach (var label in labelsForNextRpn)
            {
                if (labels.TryGetValue(label, out var command))
                {
                    var token = command.Value?.Token;
                    throw new RpnCreationException(
                        $"{label} is already used for labeling statement " +
                        $"{token?.Value} ({token?.Line}:{token?.StartPosition})"
                    );
                }

                labels.Add(label, program.Last);
            }

            labelsForNextRpn.Clear();
        }

        private void AddLabelForNextRpn(string labelName)
        {
            labelsForNextRpn.Add(new EntityName(labelName));
        }

        private void Label(string labelName, string context = "")
        {
            AddRpn(new RpnLabel(context + labelName));
        }

        private RpnBinaryOperation GetBinaryOperation(Token token, bool isReversed)
            => token.Value switch
            {
                Operations.Plus => new RpnAdd(token),
                Operations.Minus => new RpnSubtract(token),
                Operations.Multiply => new RpnMultiply(token),
                Operations.Divide => new RpnDivide(token),
                Operations.Mod => new RpnMod(token),
                Operations.Or => new RpnOr(token, isReversed),
                Operations.And => new RpnAnd(token, isReversed),
                Operations.Equal => new RpnEqual(token, isReversed),
                Operations.Greater => new RpnGreater(token, isReversed),
                Operations.Less => new RpnLess(token, isReversed),
                Operations.Cast => new RpnCast(token),
                Operations.CanCast => new RpnCheckCast(token, isReversed),
                Operations.In => new RpnIn(token, variables, isReversed),
                Operations.RightShift => new RpnRightShift(token),
                Operations.LeftShift => new RpnLeftShift(token),
                Operations.Assign => new RpnAssign(token, variables),
                Operations.Insert => new RpnInsert(token, variables),
                var op => throw new RpnCreationException("Unknown binary operation: " + op)
            };

        private void NewOperation(RpnOperation operation)
        {
            FlushOperationsWithHigherOrEqualPriority(operation);
            expressionStack.Push(operation);
        }

        private void FlushOperationsWithHigherOrEqualPriority(RpnOperation operation)
        {
            while (ShouldPopOperation())
            {
                AddRpn(expressionStack.Pop());
            }

            bool ShouldPopOperation()
            {
                if (!expressionStack.TryPeek(out var peekedOperation))
                {
                    return false;
                }

                if (peekedOperation is null)
                {
                    return false;
                }

                if (peekedOperation.HasLessPriorityThan(operation))
                {
                    return false;
                }

                return true;
            }
        }

        private void AddRpn(Rpn rpn)
        {
            program.AddLast(new LinkedListNode<Rpn>(rpn));
            if (labelsForNextRpn.Count > 0)
            {
                LabelLastRpn();
            }
        }

        /// <summary>
        /// Contains all context of the current lambda.
        /// </summary>
        private class LambdaContext
        {
            private readonly List<string> localVariables = new List<string>();
            private readonly List<string> refVariables = new List<string>();

            public LambdaContext(int lambdaIdx)
            {
                LambdaName = $"{LambdaPrefix}{lambdaIdx}>_";
                EndLabel = $"{LambdaEndPrefix}{lambdaIdx}>_";
            }

            public string LambdaName { get; }
            public string EndLabel { get; }

            public void AddLocalVariable(string name, bool isRef = false)
            {
                localVariables.Add(name);
                if (isRef)
                {
                    refVariables.Add(name);
                }
            }

            public bool HasLocalVariable(string name)
                => localVariables.Contains(name);

            public bool IsVariableRef(string name)
                => refVariables.Contains(name);
        }
    }
}