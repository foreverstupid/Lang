using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Creates the needed program interpretation info.
    /// </summary>
    public class ProgramCreator
    {
        private const string IfLabelPrefix = "#if#label#";
        private const string ElseLabelPrefix = "#else#label#";
        private const string LambdaPrefix = "#f#";
        private const string LambdaEndPrefix = "#end#of#f#";
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
        private readonly Stack<Token> parameterStack = new Stack<Token>();

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

        public void AddLabelForNextRpn(Token token)
        {
            if (token.TokenType != Token.Type.Label)
            {
                throw new RpnCreationException(
                    $"Given token {token.Value} is " +
                    $"not a label, but {token.TokenType}"
                );
            }

            AddLabelForNextRpn(token.Value);
        }

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
                    rpn = new RpnVar(token, lambdaContext + token.Value);
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

        public void Label(Token token)
        {
            AddRpn(new RpnLabel(token, lambdaContext + token.Value));
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

        public void CodeBlockStart()
        {
            OpenBracket();
            lambdaCount++;
            lambdaIdxStack.Push(lambdaCount);
            var lambdaName = LambdaPrefix + lambdaCount;
            var lambdaEndLabel = LambdaEndPrefix + lambdaCount;

            lambdaContext += lambdaName;

            // add jump to the end of the code block
            Label(lambdaEndLabel);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddRpn(new RpnNop());
            functions.Add(lambdaName, Program.Last);
        }

        public void CodeBlockFinish()
        {
            CloseBracket();
            Program.RemoveLast();   // not ignore the last valur to return it from the function

            var lambdaIdx = lambdaIdxStack.Pop();
            var lambdaEndLabel = LambdaEndPrefix + lambdaIdx;
            var lambdaName = LambdaPrefix + lambdaIdx;
            var withoutLastContext = lambdaContext
                .Substring(0, lambdaContext.Length - lambdaName.Length);

            Label(ReturnLabelPrefix, context: withoutLastContext);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddLabelForNextRpn(lambdaEndLabel);
            AddRpn(new RpnFunc(lambdaName));

            lambdaContext = withoutLastContext;
        }

        public void Parameter(Token token)
        {
            AddRpn(new RpnVar(token, lambdaContext + token.Value));
            AddRpn(new RpnRightAssign(token, variables));
            AddRpn(new RpnIgnore());
        }

        public void Ignore()
        {
            AddRpn(new RpnIgnore());
        }

        public void Goto(Token token)
        {
            AddRpn(new RpnGoto(token, labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));
        }

        public void If(Token token)
        {
            EndOfExpression();
            ifIdxStack.Push(ifCount);
            var ifLabelName = IfLabelPrefix + ifCount;

            Label(ifLabelName);
            AddRpn(new RpnIfGoto(token, labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));
            ifCount++;
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

        public void Eval(Token token, int paramCount)
        {
            var ctxt = lambdaContext;
            AddRpn(new RpnEval(
                token,
                paramCount,
                functions as IReadOnlyDictionary<string, LinkedListNode<Rpn>>,
                variables as IReadOnlyDictionary<string, RpnConst>,
                ret => labels[ctxt + ReturnLabelPrefix] = ret
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
            labelsForNextRpn.Add(lambdaContext + labelName);
        }

        private void Label(string labelName, string context = null)
        {
            context ??= lambdaContext;
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