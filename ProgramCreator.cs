using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Creates the needed program interpretation info.
    /// </summary>
    public class ProgramCreator
    {
        private readonly List<Rpn> program = new List<Rpn>();
        private readonly Dictionary<string, int> labels = new Dictionary<string, int>();
        private readonly Dictionary<string, RpnConst> variables = new Dictionary<string, RpnConst>();
        private readonly List<string> labelsForNextRpn = new List<string>();
        private readonly Stack<RpnOperation> expressionStack = new Stack<RpnOperation>();

        public ProgramCreator()
        {
            expressionStack.Push(null);
        }

        public ProgramInfo GetInfo()
        {
            return new ProgramInfo(){ Rpns = program };
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

        public void Assignment(Token token)
        {
            program.Add(new RpnAssign(token));
        }

        public void Indexator(Token token)
        {
            program.Add(new RpnIndexator(token));
        }

        public void Dereference(Token token)
        {
            program.Add(new RpnGetValue(token));
        }

        public void Literal(Token token)
        {
            RpnConst rpn = token.TokenType switch
            {
                Token.Type.Float => new RpnFloat(token),
                Token.Type.Integer => new RpnInteger(token),
                Token.Type.String => new RpnString(token),
                _ => throw new RpnCreationException($"Unexpected literal token type {token.TokenType}")
            };

            program.Add(rpn);
        }

        public void Variable(Token token)
        {
        }

        public void Label(Token token)
        {
            program.Add(new RpnLabel(token));
        }

        public void PositionalArgument(Token token)
        {

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
            program.Add(new RpnGoto(token));
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
                if (labels.TryGetValue(label, out int index))
                {
                    var rpn = program[index];
                    throw new RpnCreationException(
                        $"{label} is already used for labeling statement " +
                        $"{rpn.Token.Value} ({rpn.Token.Line}:{rpn.Token.StartPosition})"
                    );
                }

                labels.Add(label, program.Count - 1);
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
                program.Add(operation);
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
                program.Add(stackOp);
                expressionStack.Pop();
            }

            expressionStack.Push(operation);
        }
    }
}