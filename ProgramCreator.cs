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

        public ProgramInfo GetInfo()
        {
            return null;
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

        public void Assignment()
        {
            program.Add(new RpnAssign());
        }

        public void Indexator()
        {
            program.Add(new RpnGetIndex());
        }

        public void Dereference()
        {
            program.Add(new RpnGetValue());
        }

        public void Literal(Token token)
        {
            RpnConst rpn = token.TokenType switch
            {
                Token.Type.Float => new RpnFloat(),
                Token.Type.Integer => new RpnInteger(),
                Token.Type.String => new RpnString(),
                _ => throw new RpnCreationException($"Unexpected literal token type {token.TokenType}")
            };

            program.Add(rpn);
        }

        public void Variable(Token token)
        {
        }

        public void Label(Token token)
        {
            program.Add(new RpnLabel());
        }

        public void PositionalArgument(Token token)
        {

        }

        public void UnaryOperation(Token token)
        {

        }

        public void BinaryOperation(Token token)
        {

        }

        public void CodeBlockStart()
        {

        }

        public void CodeBlockFinish()
        {
            
        }

        public void Goto()
        {
            program.Add(new RpnGoto());
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
    }
}