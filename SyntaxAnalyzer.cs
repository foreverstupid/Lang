using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    /// <summary>
    /// Performs syntax analysis, creating all needed tables.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private static readonly string[] UnarOperations = new[] { "-", "!", "+" };
        private static readonly string[] BinarOperations =
            new[] { "=", "+", "-", "/", "*", "/", "&", "|", ">", "<", "~", "%", "^" };

        private readonly ILogger logger;

        private IEnumerator<Token> tokens;
        private Token previousToken;
        private bool finished = false;
        private string errorMessage;

        public SyntaxAnalyzer(ILogger logger)
        {
            this.logger = logger;
        }

        private Token Current { get; set; }

        public InterpretataionInfo Analyse(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.GetEnumerator();
            MoveNext();
            if (!Program())
            {
                throw new SyntaxException(errorMessage + $" {previousToken.Line}:{previousToken.StartPosition}");
            }

            return null;
        }

        private void MoveNext()
        {
            previousToken = Current;
            if (!tokens.MoveNext())
            {
                finished = true;
                Current = new Token("", Token.Type.None, 0, 0);
            }
            else
            {
                Current = tokens.Current;
            }

            errorMessage = null;
        }

        private void SetError(string msg)
        {
            errorMessage = msg;
        }

        private bool Program()
        {
            logger.AddContext(nameof(Program));

            while (!finished)
            {
                if (!Statement())
                {
                    return false;
                }
            }

            logger.CloseLastContext(true);
            return true;
        }

        private bool Statement()
        {
            logger.AddContext(nameof(Statement));

            if (Current.TokenType == Token.Type.Label)
            {
                MoveNext();
                if (Current.Value != ":")
                {
                    SetError("Expected colomn after label");
                    logger.CloseLastContext(false);
                    return false;
                }

                MoveNext();
                bool help = Statement();
                logger.CloseLastContext(help);
                return help;
            }
            else if (IfStatement())
            {
                logger.CloseLastContext(true);
                return true;
            }
            else if (Expression() || GotoStatement() || Assignment())
            {
                if (Current.Value != ";")
                {
                    SetError("Expected semicolon after statement");
                    logger.CloseLastContext(false);
                    return false;
                }

                MoveNext();
                logger.CloseLastContext(true);
                return true;
            }

            logger.CloseLastContext(false);
            return false;
        }

        private bool Assignment()
        {
            logger.AddContext(nameof(Assignment));
            if (!LeftValue())
            {
                logger.CloseLastContext(false);
                return false;
            }

            if (Current.Value != "=")
            {
                SetError("Expected '=' in assignment after the left value");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            bool help = Expression();
            logger.CloseLastContext(help);
            return help;
        }

        private bool LeftValue()
        {
            logger.AddContext(nameof(LeftValue));
            if (Current.TokenType != Token.Type.Identifier)
            {
                SetError("Expected identifier at the beginning if the left value");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            bool help = Tail();
            logger.CloseLastContext(help);
            return help;
        }

        private bool Tail()
        {
            logger.AddContext(nameof(Tail));
            if (Indexator() || Arguments())
            {
                bool help = Tail();
                logger.CloseLastContext(help);
                return help;
            }

            logger.CloseLastContext(true);
            return true;
        }

        private bool Indexator()
        {
            if (Current.Value != "[")
            {
                SetError("Expected '[' in the beggining of indexator");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();

            if (!Expression())
            {
                logger.CloseLastContext(false);
                return false;
            }

            if (Current.Value != "]")
            {
                SetError("Expected ']' in the end of the indexator");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            logger.CloseLastContext(true);
            return true;
        }

        private bool Arguments()
        {
            logger.AddContext(nameof(Arguments));
            if (Current.Value != "(")
            {
                SetError("Expected open paranthesis at the beginning of argument list");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            if (Current.Value == ")")
            {
                MoveNext();
                logger.CloseLastContext(true);
                return true;
            }

            if (!Expression())
            {
                logger.CloseLastContext(false);
                return false;
            }

            while (Current.Value != ")")
            {
                if (Current.Value != ",")
                {
                    SetError("Expected comma between arguments");
                    logger.CloseLastContext(false);
                    return false;
                }

                MoveNext();
                if (!Expression())
                {
                    logger.CloseLastContext(false);
                    return false;
                }
            }

            MoveNext();
            logger.CloseLastContext(true);
            return true;
        }

        private bool Expression()
        {
            logger.AddContext(nameof(Expression));
            if (IsUnarOperation(Current))
            {
                MoveNext();
            }

            if (!Operand())
            {
                logger.CloseLastContext(false);
                return false;
            }

            while (IsBinarOperation(Current))
            {
                MoveNext();
                if (!Expression())
                {
                    logger.CloseLastContext(false);
                    return false;
                }
            }

            bool help = Tail();
            logger.CloseLastContext(help);
            return help;
        }

        private bool Operand()
        {
            logger.AddContext(nameof(Operand));
            if (Current.Value == "(")
            {
                MoveNext();
                if (!Expression())
                {
                    logger.CloseLastContext(false);
                    return false;
                }

                if (Current.Value != ")")
                {
                    SetError("Closing paranthesis in the expression expected");
                    logger.CloseLastContext(false);
                    return false;
                }

                MoveNext();
                logger.CloseLastContext(true);
                return true;
            }

            if (Current.Value == "$")
            {
                MoveNext();
                bool help = Dereference();
                logger.CloseLastContext(help);
                return help;
            }

            bool flag = Literal();
            logger.CloseLastContext(flag);
            return flag;
        }

        private bool Dereference()
        {
            logger.AddContext(nameof(Dereference));
            if (Current.TokenType == Token.Type.Integer)
            {
                MoveNext();
                logger.CloseLastContext(true);
                return true;
            }

            bool help = LeftValue();
            logger.CloseLastContext(help);
            return help;
        }

        private bool Literal()
        {
            logger.AddContext(nameof(Literal));
            if (Current.TokenType == Token.Type.Integer ||
                Current.TokenType == Token.Type.Float ||
                Current.TokenType == Token.Type.String)
            {
                MoveNext();
                logger.CloseLastContext(true);
                return true;
            }

            bool help = CodeBlock();
            logger.CloseLastContext(help);
            return help;
        }

        private bool CodeBlock()
        {
            logger.AddContext(nameof(CodeBlock));
            if (Current.Value != "{")
            {
                SetError("Expected opening curly bracket at the start of the code block");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            while (Statement())
            {
            }

            if (Current.Value != "}")
            {
                SetError("Expected closing curly bracket at the end of the code block");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            logger.CloseLastContext(true);
            return true;
        }

        private bool GotoStatement()
        {
            logger.AddContext(nameof(GotoStatement));
            if (Current.Value != KeyWords.Goto)
            {
                SetError($"Expected '{KeyWords.Goto}' at the beginning of the goto statement");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            if (Current.TokenType != Token.Type.Label)
            {
                SetError("Expected label in the goto statement");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            logger.CloseLastContext(true);
            return true;
        }

        private bool IfStatement()
        {
            logger.AddContext(nameof(IfStatement));
            if (Current.Value != KeyWords.If)
            {
                SetError($"Expected {KeyWords.If} at the beginning of the if-statement");
                logger.CloseLastContext(false);
                return false;
            }

            MoveNext();
            if (!Expression())
            {
                SetError("Expected expression in the condition part");
                logger.CloseLastContext(false);
                return false;
            }

            if (!CodeBlock())
            {
                logger.CloseLastContext(false);
                return false;
            }

            if (Current.Value == KeyWords.Else)
            {
                MoveNext();
                if (!CodeBlock())
                {
                    logger.CloseLastContext(false);
                    return false;
                }
            }

            logger.CloseLastContext(true);
            return true;
        }

        private bool IsUnarOperation(Token token)
        {
            return
                token.TokenType == Token.Type.Separator &&
                UnarOperations.Contains(token.Value);
        }

        private bool IsBinarOperation(Token token)
        {
            return
                token.TokenType == Token.Type.Separator &&
                BinarOperations.Contains(token.Value);
        }
    }
}