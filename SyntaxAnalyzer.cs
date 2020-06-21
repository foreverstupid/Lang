using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    /// <summary>
    /// Performs syntax analysis and preliminary interpretation info cration.
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

        private List<string> contexts = new List<string>();

        public SyntaxAnalyzer(ILogger logger)
        {
            this.logger = logger.ForContext("Syntax analysis");
        }

        private Token Current { get; set; }

        /// <summary>
        /// Performs analysis, creating needed interpratation info.
        /// </summary>
        /// <param name="tokens">The collection of the code tokens.</param>
        /// <returns>Needed information for interpretation.</returns>
        public InterpretataionInfo Analyse(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.GetEnumerator();
            MoveNext();

            if (!Program())
            {
                throw new SyntaxException(errorMessage);
            }

            return null;
        }

        /// <summary>
        /// Moves one step forward in a token collection. Performs additional RPN
        /// and other structures creation.
        /// </summary>
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

            errorMessage = "";
        }

        /// <summary>
        /// Sets the error of the syntax analysis.
        /// </summary>
        /// <param name="msg">Error message.</param>
        private void SetError(string msg)
        {
            errorMessage += $"({Current.Line}:{Current.StartPosition}) {msg}\n";
        }

        /// <summary>
        /// Actions on entering into the grammar node.
        /// </summary>
        /// <param name="context">The context of the node.</param>
        private void Enter(string context)
        {
            logger.ForContext(context).Information("Entered");
            contexts.Add(context);
            errorMessage = "";
        }

        /// <summary>
        /// Actions on leaving the last grammar node.
        /// </summary>
        /// <param name="isSuccessful">Status of the current node analysis.</param>
        /// <returns>The given node analysis status.</returns>
        private bool Leave(bool isSuccessful)
        {
            var context = contexts.Last();
            contexts.RemoveAt(contexts.Count - 1);
            var logger = this.logger.ForContext(context);

            if (isSuccessful)
            {
                logger.Information("+");
                errorMessage = "";
            }
            else
            {
                logger.Error("-");
            }

            return isSuccessful;
        }

        /// <summary>
        /// Checks whether the given token is an unary operation.
        /// </summary>
        private bool IsUnaryOperation(Token token)
        {
            return
                token.TokenType == Token.Type.Separator &&
                UnarOperations.Contains(token.Value);
        }

        /// <summary>
        /// Checks whether the given token is a binary operation.
        /// </summary>
        private bool IsBinaryOperation(Token token)
        {
            return
                token.TokenType == Token.Type.Separator &&
                BinarOperations.Contains(token.Value);
        }

        // Grammar nodes handlers

        private bool Program()
        {
            Enter(nameof(Program));

            while (!finished)
            {
                if (!Statement())
                {
                    SetError("Valid statement is expected");
                    return Leave(false);
                }
            }

            return Leave(true);
        }

        private bool Statement()
        {
            Enter(nameof(Statement));

            if (Current.TokenType == Token.Type.Label)
            {
                MoveNext();
                if (Current.Value != ":")
                {
                    SetError("Colomn after label is expected");
                    return Leave(false);
                }

                MoveNext();
                return Leave(Statement());
            }
            else if (IfStatement())
            {
                return Leave(true);
            }
            else if (Expression() || GotoStatement() || Assignment())
            {
                if (Current.Value != ";")
                {
                    SetError("Semicolon after statement is expected");
                    return Leave(false);
                }

                MoveNext();
                return Leave(true);
            }

            SetError("Unknown type of the statement");
            return Leave(false);
        }

        private bool Assignment()
        {
            Enter(nameof(Assignment));

            if (!LeftValue())
            {
                SetError("Left-value expression in assignment is expected");
                return Leave(false);
            }

            if (Current.Value != "=")
            {
                SetError("'=' in assignment after the left-value expression is expected");
                return Leave(false);
            }

            MoveNext();
            return Leave(Expression());
        }

        private bool LeftValue()
        {
            Enter(nameof(LeftValue));

            if (Current.TokenType != Token.Type.Identifier)
            {
                SetError("Identifier at the beginning if the left-value expression is expected");
                return Leave(false);
            }

            MoveNext();
            return Leave(Tail());
        }

        private bool Tail()
        {
            Enter(nameof(Tail));

            if (Indexator() || Arguments())
            {
                return Leave(Tail());
            }

            return Leave(true);
        }

        private bool Indexator()
        {
            Enter(nameof(Indexator));

            if (Current.Value != "[")
            {
                SetError("'[' at the beggining of indexator is expected");
                return Leave(false);
            }

            MoveNext();

            if (!Expression())
            {
                SetError("Expression in the indexator is expected");
                return Leave(false);
            }

            if (Current.Value != "]")
            {
                SetError("']' in the end of the indexator is expected");
                return Leave(false);
            }

            MoveNext();
            return Leave(true);
        }

        private bool Arguments()
        {
            Enter(nameof(Arguments));

            if (Current.Value != "(")
            {
                SetError("Opening paranthesis at the beginning of argument list is expected");
                return Leave(false);
            }

            MoveNext();
            if (Current.Value == ")")
            {
                MoveNext();
                return Leave(true);
            }

            if (!Expression())
            {
                SetError(
                    "The first argument of the non-empty argument list is expected to be " +
                    "an expression"
                );

                return Leave(false);
            }

            while (Current.Value != ")")
            {
                if (Current.Value != ",")
                {
                    SetError("Comma between arguments is expected");
                    return Leave(false);
                }

                MoveNext();
                if (!Expression())
                {
                    SetError("Expression in an argument list is expected");
                    return Leave(false);
                }
            }

            MoveNext();
            return Leave(true);
        }

        private bool Expression()
        {
            Enter(nameof(Expression));

            if (IsUnaryOperation(Current))
            {
                MoveNext();
            }

            if (!Operand())
            {
                SetError("The operand of the expression is expected");
                return Leave(false);
            }

            while (IsBinaryOperation(Current))
            {
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression as a binary operation right operand is expected");
                    return Leave(false);
                }
            }

            return Leave(Tail());
        }

        private bool Operand()
        {
            Enter(nameof(Operand));

            if (Current.Value == "(")
            {
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after opening parathesis is expected");
                    return Leave(false);
                }

                if (Current.Value != ")")
                {
                    SetError("Closing paranthesis in the expression is expected");
                    return Leave(false);
                }

                MoveNext();
                return Leave(true);
            }

            if (Current.Value == "$")
            {
                MoveNext();
                return Leave(Dereference());
            }

            return Leave(Literal());
        }

        private bool Dereference()
        {
            Enter(nameof(Dereference));

            if (Current.TokenType == Token.Type.Integer)
            {
                MoveNext();
                return Leave(true);
            }

            return Leave(LeftValue());
        }

        private bool Literal()
        {
            Enter(nameof(Literal));

            if (Current.TokenType == Token.Type.Integer ||
                Current.TokenType == Token.Type.Float ||
                Current.TokenType == Token.Type.String)
            {
                MoveNext();
                return Leave(true);
            }

            return Leave(CodeBlock());
        }

        private bool CodeBlock()
        {
            Enter(nameof(CodeBlock));

            if (Current.Value != "{")
            {
                SetError("Opening curly bracket at the start of the code block is expected");
                return Leave(false);
            }

            MoveNext();
            while (Statement())
            {
            }

            if (Current.Value != "}")
            {
                SetError("Closing curly bracket at the end of the code block is expected");
                return Leave(false);
            }

            MoveNext();
            return Leave(true);
        }

        private bool GotoStatement()
        {
            Enter(nameof(GotoStatement));

            if (Current.Value != KeyWords.Goto)
            {
                SetError(
                    $"'{KeyWords.Goto}' key word at the beginning if the " +
                    "goto-statement is expected"
                );

                return Leave(false);
            }

            MoveNext();
            if (Current.TokenType != Token.Type.Label)
            {
                SetError("Label in the goto statement is expected");
                return Leave(false);
            }

            MoveNext();
            return Leave(true);
        }

        private bool IfStatement()
        {
            Enter(nameof(IfStatement));

            if (Current.Value != KeyWords.If)
            {
                SetError(
                    $"'{KeyWords.If}' key word at the beginning if the " +
                    "if-statement is expected"
                );

                return Leave(false);
            }

            MoveNext();
            if (!Expression())
            {
                return Leave(false);
            }

            if (!CodeBlock())
            {
                SetError("Code block in the if-part is expected");
                return Leave(false);
            }

            if (Current.Value == KeyWords.Else)
            {
                MoveNext();
                if (!CodeBlock())
                {
                    SetError("Code block in the else-part is expected");
                    return Leave(false);
                }
            }

            return Leave(true);
        }
    }
}