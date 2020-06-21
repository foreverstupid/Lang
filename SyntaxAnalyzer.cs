using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    /// <summary>
    /// Performs syntax analysis and preliminary interpretation info cration.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private readonly ILogger logger;
        private TokenEnumerator tokens;
        private List<string> contexts = new List<string>();

        public SyntaxAnalyzer(ILogger logger)
        {
            this.logger = logger.ForContext("Syntax analysis");
        }

        /// <summary>
        /// Performs analysis, creating needed interpratation info.
        /// </summary>
        /// <param name="tokens">The collection of the code tokens.</param>
        /// <returns>Needed information for interpretation.</returns>
        public InterpretataionInfo Analyse(IEnumerable<Token> tokens)
        {
            this.tokens = new TokenEnumerator(tokens);

            try
            {
                Program();

                if (!this.tokens.IsFinished)
                {
                    SetError("Unknown statement");
                }
            }
            catch (SyntaxException se)
            {
                logger.Error(se.Message);
            }

            return null;
        }

        /// <summary>
        /// Sets the error of the syntax analysis.
        /// </summary>
        /// <param name="msg">Error message.</param>
        private void SetError(string msg)
        {
            var errorMessage = $"({tokens.CurrentOrLast.Line}:{tokens.CurrentOrLast.StartPosition}) {msg}";
            throw new SyntaxException(errorMessage);
        }

        /// <summary>
        /// Actions on entering into the grammar node.
        /// </summary>
        /// <param name="context">The context of the node.</param>
        private void Enter(string context)
        {
            logger.ForContext(context).Information("Entered");
            contexts.Add(context);
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
            }
            else
            {
                logger.Error("-");
            }

            return isSuccessful;
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        private void MoveNext()
        {
            logger.ForContext(contexts[^1]).Information(tokens.CurrentOrLast.Value);
            tokens.MoveNext();
        }

        // Grammar nodes handlers

        private bool Program()
        {
            Enter(nameof(Program));

            while (Statement())
            {
            }

            return Leave(true);
        }

        private bool Statement()
        {
            Enter(nameof(Statement));

            if (tokens.CurrentTokenTypeIs(Token.Type.Label))
            {
                MoveNext();
                if (!tokens.CurrentTokenValueIs(":"))
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
                if (!tokens.CurrentTokenValueIs(";"))
                {
                    SetError("Semicolon after statement is expected");
                    return Leave(false);
                }

                MoveNext();
                return Leave(true);
            }

            return Leave(false);
        }

        private bool Assignment()
        {
            Enter(nameof(Assignment));

            if (!LeftValue())
            {
                return Leave(false);
            }

            if (!tokens.CurrentTokenValueIs("="))
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

            if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
            {
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

            if (!tokens.CurrentTokenValueIs("["))
            {
                return false;
            }

            MoveNext();

            if (!Expression())
            {
                SetError("Expression in the indexator is expected");
            }

            if (!tokens.CurrentTokenValueIs("]"))
            {
                SetError("']' in the end of the indexator is expected");
            }

            MoveNext();
            return Leave(true);
        }

        private bool Arguments()
        {
            Enter(nameof(Arguments));

            if (!tokens.CurrentTokenValueIs("("))
            {
                return Leave(false);
            }

            MoveNext();
            if (tokens.CurrentTokenValueIs(")"))
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
            }

            while (!tokens.CurrentTokenValueIs(")"))
            {
                if (!tokens.CurrentTokenValueIs(","))
                {
                    SetError("Comma between arguments is expected");
                }

                MoveNext();
                if (!Expression())
                {
                    SetError("Expression in an argument list is expected");
                }
            }

            MoveNext();
            return Leave(true);
        }

        private bool Expression()
        {
            Enter(nameof(Expression));

            if (tokens.CurrentTokenIsUnaryOperation())
            {
                MoveNext();
            }

            if (!Operand())
            {
                return Leave(false);
            }

            while (tokens.CurrentTokenIsBinaryOperation())
            {
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression as a binary operation right operand is expected");
                }
            }

            return Leave(Tail());
        }

        private bool Operand()
        {
            Enter(nameof(Operand));

            if (tokens.CurrentTokenValueIs("("))
            {
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after opening parathesis is expected");
                }

                if (!tokens.CurrentTokenValueIs(")"))
                {
                    SetError("Closing paranthesis in the expression is expected");
                }

                MoveNext();
                return Leave(true);
            }

            if (Dereference())
            {
                return Leave(true);
            }

            return Leave(Literal());
        }

        private bool Dereference()
        {
            Enter(nameof(Dereference));

            if (!tokens.CurrentTokenValueIs("$"))
            {
                return false;
            }

            MoveNext();
            if (tokens.CurrentTokenTypeIs(Token.Type.Integer))
            {
                MoveNext();
                return Leave(true);
            }

            return Leave(LeftValue());
        }

        private bool Literal()
        {
            Enter(nameof(Literal));

            if (tokens.CurrentTokenTypeIs(Token.Type.Integer) ||
                tokens.CurrentTokenTypeIs(Token.Type.Float) ||
                tokens.CurrentTokenTypeIs(Token.Type.String))
            {
                MoveNext();
                return Leave(true);
            }

            return Leave(CodeBlock());
        }

        private bool CodeBlock()
        {
            Enter(nameof(CodeBlock));

            if (!tokens.CurrentTokenValueIs("{"))
            {
                return Leave(false);
            }

            MoveNext();
            if (!Program())
            {
                SetError("Invalid code block body");
            }

            if (!tokens.CurrentTokenValueIs("}"))
            {
                SetError("Closing curly bracket at the end of the code block is expected");
            }

            MoveNext();
            return Leave(true);
        }

        private bool GotoStatement()
        {
            Enter(nameof(GotoStatement));

            if (!tokens.CurrentTokenValueIs(KeyWords.Goto))
            {
                return Leave(false);
            }

            MoveNext();
            if (!tokens.CurrentTokenTypeIs(Token.Type.Label))
            {
                SetError("Label in the goto statement is expected");
            }

            MoveNext();
            return Leave(true);
        }

        private bool IfStatement()
        {
            Enter(nameof(IfStatement));

            if (!tokens.CurrentTokenValueIs(KeyWords.If))
            {
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
            }

            if (tokens.CurrentTokenValueIs(KeyWords.Else))
            {
                MoveNext();
                if (!CodeBlock())
                {
                    SetError("Code block in the else-part is expected");
                }
            }

            return Leave(true);
        }
    }
}