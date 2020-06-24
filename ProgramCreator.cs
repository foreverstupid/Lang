using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Creates the needed program interpretation info.
    /// </summary>
    public class ProgramCreator
    {
        private readonly LinkedList<Rpn> program = new LinkedList<Rpn>();
        private readonly Dictionary<string, LinkedListNode<Rpn>> labels =
            new Dictionary<string, LinkedListNode<Rpn>>();

        private readonly Dictionary<string, Variable> variables =
            new Dictionary<string, Variable>();

        private readonly List<string> labelsForNextRpn = new List<string>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();

        public ProgramCreator()
        {
            expressionStack.Push(null);
        }

        public ProgramInfo GetInfo()
        {
            var linkedList = new LinkedList<Rpn>();
            foreach (var rpn in program)
            {
                linkedList.AddLast(new LinkedListNode<Rpn>(rpn));
            }

            return new ProgramInfo()
            {
                Rpns = linkedList
            };
        }

        public void MarkNextRpn(Token token)
        {
            if (token.TokenType != Token.Type.Label)
            {
                throw new RpnCreationException(
                    $"Given token {token.Value} is " +
                    $"not a label, but {token.TokenType}"
                );
            }

            labelsForNextRpn.Add(token.Value);
        }

        public void Indexator(Token token)
        {
            NewOperation(new RpnIndexator(token));
        }

        public void Literal(Token token)
        {
            RpnConst rpn = null;

            if (token.TokenType == Token.Type.Identifier)
            {
                if (variables.TryGetValue(token.Value, out var variable))
                {
                    rpn = new RpnVar(token, variable);
                }
                else
                {
                    var newVarValue = new Variable();
                    variables.Add(token.Value, newVarValue);
                    rpn = new RpnVar(token, newVarValue);
                }
            }
            else
            {
                rpn = token.TokenType switch
                {
                    Token.Type.Float => new RpnFloat(token),
                    Token.Type.Integer => new RpnInteger(token),
                    Token.Type.String => new RpnString(token),
                    _ => throw new RpnCreationException($"Unexpected literal token type {token.TokenType}")
                };
            }

            AddRpn(rpn);
        }

        public void Label(Token token)
        {
            //AddRpn(new RpnLabel(token, rpn));
        }

        public void UnaryOperation(Token token)
        {
            RpnOperation rpn = token.Value switch
            {
                "!" => new RpnNot(token),
                "-" => new RpnNegate(token),
                "$" => new RpnGetValue(token),
                var op => throw new RpnCreationException("Unknown unary operation: " + op)
            };

            NewOperation(rpn);
        }

        public void BinaryOperation(Token token)
        {
            RpnOperation rpn = token.Value switch
            {
                "=" => new RpnAssign(token),
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

        public void Goto(Token token)
        {
            AddRpn(new RpnGoto(token));
        }

        public void IfBlock()
        {

        }

        public void ElseBlock()
        {

        }

        public void If()
        {

        }

        public void Eval()
        {
            
        }

        private void LabelLastRpn()
        {
            foreach (var label in labelsForNextRpn)
            {
                if (labels.TryGetValue(label, out var command))
                {
                    var token = command.Value.Token;
                    throw new RpnCreationException(
                        $"{label} is already used for labeling statement " +
                        $"{token.Value} ({token.Line}:{token.StartPosition})"
                    );
                }

                labels.Add(label, program.Last);
            }

            labelsForNextRpn.Clear();
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

        private void NewOperation(RpnOperation operation)
        {
            while (expressionStack.TryPeek(out var stackOp) &&
                !(stackOp is null) &&
                !(stackOp.HasLessPriorityThan(operation)))
            {
                AddRpn(stackOp);
                expressionStack.Pop();
            }

            expressionStack.Push(operation);
        }

        private void AddRpn(Rpn rpn) => program.AddLast(new LinkedListNode<Rpn>(rpn));
    }
}