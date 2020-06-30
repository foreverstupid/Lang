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

        private readonly IDictionary<string, RpnConst> variables =
            new Dictionary<string, RpnConst>();

        private readonly IDictionary<string, LinkedListNode<Rpn>> labels =
            new Dictionary<string, LinkedListNode<Rpn>>();

        private readonly IDictionary<string, LinkedListNode<Rpn>> functions =
            new Dictionary<string, LinkedListNode<Rpn>>();

        private readonly List<string> labelsForNextRpn = new List<string>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();
        private readonly Stack<int> ifIdxStack = new Stack<int>();
        private readonly Stack<int> lambdaIdxStack = new Stack<int>();
        private readonly Stack<List<string>> parametersStack = new Stack<List<string>>();

        private int ifCount = 0;
        private int lambdaCount = 0;
        private string lambdaContext = "";

        public ProgramCreator()
        {
            expressionStack.Push(null);
        }

        /// <summary>
        /// Program representation as RPNs.
        /// </summary>
        public LinkedList<Rpn> Program { get; } = new LinkedList<Rpn>();

        public void Indexator(Token token)
        {
            NewOperation(new RpnIndexator(token));
        }

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

        public void UnaryOperation(Token token)
        {
            RpnOperation rpn = token.Value switch
            {
                "!" => new RpnNot(token),
                "-" => new RpnNegate(token),
                "$" => new RpnGetValue(token, variables as IReadOnlyDictionary<string, RpnConst>),
                var op => throw new RpnCreationException("Unknown unary operation: " + op)
            };

            NewOperation(rpn);
        }

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
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddRpn(new RpnNop());
            functions.Add(lambdaName, Program.Last);
        }

        public void LambdaFinish()
        {
            parametersStack.Pop();
            CloseBracket();

            var lambdaIdx = lambdaIdxStack.Pop();
            var lambdaEndLabel = LambdaEndPrefix + lambdaIdx;
            var lambdaName = LambdaPrefix + lambdaIdx;

            Label(ReturnLabelPrefix, lambdaName);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddLabelForNextRpn(lambdaEndLabel);
            AddRpn(new RpnFunc(lambdaName));
        }

        public void Parameter(Token token)
        {
            parametersStack.Peek().Add(token.Value);
            AddRpn(new RpnVar(token, lambdaContext + token.Value));
            AddRpn(new RpnRightAssign(token, variables));
            AddRpn(new RpnIgnore());
        }

        public void Ignore(Token token)
        {
            AddRpn(new RpnIgnore(token));
        }

        public void If(Token token)
        {
            ifCount++;
            ifIdxStack.Push(ifCount);
            var ifLabelName = IfLabelPrefix + ifCount;

            Label(ifLabelName);
            AddRpn(new RpnIfGoto(token, labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));
        }

        public void Else(Token token)
        {
            var idx = ifIdxStack.Peek();
            var elseLabelName = ElseLabelPrefix + idx;
            var ifLabelName = IfLabelPrefix + idx;

            Label(elseLabelName);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddLabelForNextRpn(ifLabelName);
        }

        public void EndIf()
        {
            var idx = ifIdxStack.Pop();
            var ifLabelName = IfLabelPrefix + idx;
            AddLabelForNextRpn(ifLabelName);
            AddRpn(new RpnNop());
        }

        public void EndElse()
        {
            var idx = ifIdxStack.Pop();
            var elseLabelName = ElseLabelPrefix + idx;
            AddLabelForNextRpn(elseLabelName);
            AddRpn(new RpnNop());
        }

        public void CycleStart()
        {
            ifCount++;
            ifIdxStack.Push(ifCount);
            var cycleStartLabelName = IfLabelPrefix + ifCount;

            AddRpn(new RpnNone());
            AddLabelForNextRpn(cycleStartLabelName);
        }

        public void While(Token token)
        {
            var idx = ifIdxStack.Peek();
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleEndLabelName);
            AddRpn(new RpnIfGoto(token, labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));
            AddRpn(new RpnIgnore());    // ignores the value of the last iteration
        }

        public void CycleEnd()
        {
            var idx = ifIdxStack.Pop();
            var cycleStartLabelName = IfLabelPrefix + idx;
            var cycleEndLabelName = ElseLabelPrefix + idx;

            Label(cycleStartLabelName);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddLabelForNextRpn(cycleEndLabelName);
            AddRpn(new RpnNop());
        }

        public void Eval(Token token, int paramCount)
        {
            AddRpn(new RpnEval(
                token,
                paramCount,
                functions as IReadOnlyDictionary<string, LinkedListNode<Rpn>>,
                variables as IReadOnlyDictionary<string, RpnConst>,
                (funcName, ret) => labels[funcName + ReturnLabelPrefix] = ret
            ));
        }

        public void OpenBracket()
        {
            expressionStack.Push(null);
        }

        public void CloseBracket()
        {
            while (expressionStack.TryPop(out var operation) && !(operation is null))
            {
                AddRpn(operation);
            }
        }

        public void EndOfStatement()
        {
            CloseBracket();
            if (expressionStack.TryPeek(out _))
            {
                //throw new RpnCreationException("Expression is invalid");
            }

            expressionStack.Push(null);
        }

        public void EndOfExpression()
        {
            CloseBracket();
            if (expressionStack.TryPeek(out _))
            {
               // throw new RpnCreationException("Expression is invalid");
            }

            expressionStack.Push(null);
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

                labels.Add(label, Program.Last);
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
            Program.AddLast(new LinkedListNode<Rpn>(rpn));
            if (labelsForNextRpn.Count > 0)
            {
                LabelLastRpn();
            }
        }
    }
}