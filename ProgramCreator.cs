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

        private readonly LinkedList<Rpn> program = new LinkedList<Rpn>();
        private readonly IDictionary<string, RpnConst> variables =
            new Dictionary<string, RpnConst>();
        private readonly IDictionary<string, LinkedListNode<Rpn>> labels =
            new Dictionary<string, LinkedListNode<Rpn>>();

        private readonly List<string> labelsForNextRpn = new List<string>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();

        private int ifCount = 0;
        private string ifLabelName;
        private string endIfLabelName;

        public ProgramCreator()
        {
            expressionStack.Push(null);
        }

        public ProgramInfo GetInfo()
        {
            return new ProgramInfo(program);
        }

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
                    rpn = new RpnVar(token, token.Value);
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
            AddRpn(new RpnLabel(token, token.Value));
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
                var op => throw new RpnCreationException("Unknown binary operation: " + op)
            };

            NewOperation(rpn);
        }

        public void CodeBlockStart()
        {

        }

        public void CodeBlockFinish()
        {
            
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
            ifCount++;
            endIfLabelName = ifLabelName = IfLabelPrefix + ifCount;

            Label(ifLabelName);
            AddRpn(new RpnIfGoto(token, labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));
        }

        public void Else(Token token)
        {
            endIfLabelName = ElseLabelPrefix + ifCount;
            Label(endIfLabelName);
            AddRpn(new RpnGoto(labels as IReadOnlyDictionary<string, LinkedListNode<Rpn>>));

            AddLabelForNextRpn(ifLabelName);
        }

        public void EndIf()
        {
            AddLabelForNextRpn(endIfLabelName);
            AddRpn(new RpnNop());
        }

        public void Eval(Token token, int paramCount)
        {
            AddRpn(new RpnEval(token, paramCount, null));
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
                throw new RpnCreationException("Expression is invalid");
            }

            expressionStack.Push(null);
        }

        public void EndOfExpression()
        {
            CloseBracket();
            if (expressionStack.TryPeek(out _))
            {
                throw new RpnCreationException("Expression is invalid");
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

                labels.Add(label, program.Last);
            }

            labelsForNextRpn.Clear();
        }

        private void AddLabelForNextRpn(string labelName)
        {
            labelsForNextRpn.Add(labelName);
        }

        private void Label(string labelName)
        {
            AddRpn(new RpnLabel(labelName));
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