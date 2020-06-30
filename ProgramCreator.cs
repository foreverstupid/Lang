using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Creates the needed program interpretation info.
    /// </summary>
    public class ProgramCreator
    {
        private const string IfLabelPrefix = "#if#";
        private const string ElseLabelPrefix = "#else#";
        private const string LambdaPrefix = "#f#";
        private const string LambdaEndPrefix = "#f#end#";
        private const string ReturnLabelPrefix = "#return#";

        // help variables
        private readonly List<string> labelsForNextRpn = new List<string>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();
        private readonly Stack<int> ifIdxStack = new Stack<int>();
        private readonly Stack<int> lambdaIdxStack = new Stack<int>();
        private readonly Stack<List<string>> parametersStack = new Stack<List<string>>();

        private int ifCount = 0;
        private int lambdaCount = 0;
        private string lambdaContext = "";

        // creating program information
        private LinkedList<Rpn> program;
        private Dictionary<string, RpnConst> variables;
        private Dictionary<string, LinkedListNode<Rpn>> labels;
        private Dictionary<string, LinkedListNode<Rpn>> lambdas;

        /// <summary>
        /// Starts a new program creation.
        /// </summary>
        public void StartProgramCreation()
        {
            program = new LinkedList<Rpn>();
            variables = new Dictionary<string, RpnConst>();
            labels = new Dictionary<string, LinkedListNode<Rpn>>();
            lambdas = new Dictionary<string, LinkedListNode<Rpn>>();

            labelsForNextRpn.Clear();
            expressionStack.Clear();
            ifIdxStack.Clear();
            lambdaIdxStack.Clear();
            parametersStack.Clear();
        }

        /// <summary>
        /// Finishes the program creation.
        /// </summary>
        /// <returns>The program RPN representation.</returns>
        public LinkedList<Rpn> FinishProgramCreation()
        {
            if (expressionStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, expression stack is not empty"
                );
            }

            if (parametersStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, lambda parameters stack is not empty"
                );
            }

            if (ifIdxStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, 'if' stack is not empty"
                );
            }

            if (lambdaIdxStack.Count > 0)
            {
                throw new RpnCreationException(
                    "Invalid expression, lambda stack is not empty"
                );
            }

            labelsForNextRpn.Clear();
            return program;
        }

        /// <summary>
        /// creates an indexator.
        /// </summary>
        public void Indexator(Token token)
        {
            NewOperation(new RpnIndexator(token));
        }

        /// <summary>
        /// Creates a new program entity that is represented as literal.
        /// </summary>
        public void Literal(Token token)
        {
            RpnConst rpn;

            if (token.TokenType == Token.Type.Identifier)
            {
                if (BuiltIns.Funcs.ContainsKey(token.Value))
                {
                    rpn = new RpnBuiltIn(token, token.Value);
                }
                else
                {
                    var name = token.Value;
                    if (parametersStack.TryPeek(out var ps) && ps.Contains(name))
                    {
                        name = lambdaContext + name;
                    }

                    rpn = new RpnVar(token, name);
                }
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
                "!" => new RpnNot(token),
                "-" => new RpnNegate(token),
                "$" => new RpnGetValue(token, variables),
                var op => throw new RpnCreationException("Unknown unary operation: " + op)
            };

            NewOperation(rpn);
        }

        /// <summary>
        /// Adds a new binary operation.
        /// </summary>
        public void BinaryOperation(Token token)
        {
            RpnOperation rpn = token.Value switch
            {
                "=" => new RpnAssign(token, variables),
                "+" => new RpnAdd(token),
                "-" => new RpnSubtract(token),
                "*" => new RpnMultiply(token),
                "/" => new RpnDivide(token),
                "%" => new RpnMod(token),
                "|" => new RpnOr(token),
                "&" => new RpnAnd(token),
                "~" => new RpnEqual(token),
                ">" => new RpnGreater(token),
                "<" => new RpnLess(token),
                "?" => new RpnCast(token),
                "->" => new RpnRightAssign(token, variables),
                var op => throw new RpnCreationException("Unknown binary operation: " + op)
            };

            NewOperation(rpn);
        }

        /// <summary>
        /// Starts creating of a lambda.
        /// </summary>
        public void LambdaStart()
        {
            parametersStack.Push(new List<string>());
            OpenBracket();
            lambdaCount++;
            lambdaIdxStack.Push(lambdaCount);
            var lambdaName = lambdaContext = LambdaPrefix + lambdaCount;
            var lambdaEndLabel = LambdaEndPrefix + lambdaCount;

            // add jump to the end of the code block
            Label(lambdaEndLabel);
            AddRpn(new RpnGoto(labels));

            AddRpn(new RpnNop());
            lambdas.Add(lambdaName, program.Last);
        }

        /// <summary>
        /// Finishes creating of a lambda
        /// </summary>
        public void LambdaFinish()
        {
            parametersStack.Pop();
            CloseBracket();

            var lambdaIdx = lambdaIdxStack.Pop();
            var lambdaEndLabel = LambdaEndPrefix + lambdaIdx;
            var lambdaName = LambdaPrefix + lambdaIdx;

            Label(ReturnLabelPrefix, lambdaName);
            AddRpn(new RpnGoto(labels));

            AddLabelForNextRpn(lambdaEndLabel);
            AddRpn(new RpnFunc(lambdaName));

            if (lambdaIdxStack.TryPeek(out var idx))
            {
                lambdaContext = LambdaPrefix + idx;
            }
            else
            {
                lambdaContext = "";
            }
        }

        /// <summary>
        /// Creates an initializer of a lambda parameter.
        /// </summary>
        public void Parameter(Token token)
        {
            parametersStack.Peek().Add(token.Value);
            AddRpn(new RpnVar(token, lambdaContext + token.Value));
            AddRpn(new RpnRightAssign(token, variables));
            AddRpn(new RpnIgnore());
        }

        /// <summary>
        /// Adds expression value ignoring RPN.
        /// </summary>
        public void Ignore(Token token)
        {
            AddRpn(new RpnIgnore(token));
        }

        /// <summary>
        /// Starts creating if-expression.
        /// </summary>
        public void If(Token token)
        {
            ifCount++;
            ifIdxStack.Push(ifCount);
            var ifLabelName = IfLabelPrefix + ifCount;

            Label(ifLabelName);
            AddRpn(new RpnIfGoto(token, labels));
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
            ifIdxStack.Push(ifCount);
            var cycleStartLabelName = IfLabelPrefix + ifCount;

            AddRpn(new RpnNone());
            AddLabelForNextRpn(cycleStartLabelName);
        }

        /// <summary>
        /// Creates condition checking part of the cycle.
        /// </summary>
        public void While(Token token)
        {
            var idx = ifIdxStack.Peek();
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleEndLabelName);
            AddRpn(new RpnIfGoto(token, labels));
            AddRpn(new RpnIgnore());    // ignores the value of the last iteration
        }

        /// <summary>
        /// Finishes creating od the cycle.
        /// </summary>
        public void CycleEnd()
        {
            var idx = ifIdxStack.Pop();
            var cycleStartLabelName = IfLabelPrefix + idx;
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleStartLabelName);
            AddRpn(new RpnGoto(labels));

            AddLabelForNextRpn(cycleEndLabelName);
            AddRpn(new RpnNop());
        }

        /// <summary>
        /// Adds evaluation token for the function of the certain amount of parameters.
        /// </summary>
        public void Eval(Token token, int paramCount)
        {
            AddRpn(new RpnEval(
                token,
                paramCount,
                lambdas,
                variables,
                (funcName, ret) => labels[funcName + ReturnLabelPrefix] = ret
            ));
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
            labelsForNextRpn.Add(labelName);
        }

        private void Label(string labelName, string context = "")
        {
            AddRpn(new RpnLabel(context + labelName));
        }

        private void NewOperation(RpnOperation operation)
        {
            while (expressionStack.TryPeek(out var stackOp) &&
                !(stackOp is null) &&
                !(stackOp.HasLessPriorityThan(operation)))
            {
                AddRpn(expressionStack.Pop());
            }

            expressionStack.Push(operation);
        }

        private void AddRpn(Rpn rpn)
        {
            program.AddLast(new LinkedListNode<Rpn>(rpn));
            if (labelsForNextRpn.Count > 0)
            {
                LabelLastRpn();
            }
        }
    }
}