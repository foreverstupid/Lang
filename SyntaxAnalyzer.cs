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
        private Stack<string> contexts = new Stack<string>();
        private ProgramCreator creator;

        public SyntaxAnalyzer(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Performs analysis, creating needed interpratation info.
        /// </summary>
        /// <param name="tokens">The collection of the code tokens.</param>
        /// <returns>Needed information for interpretation.</returns>
        public ProgramInfo Analyse(IEnumerable<Token> tokens)
        {
            this.tokens = new TokenEnumerator(tokens);
            creator = new ProgramCreator();

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

            return creator.GetInfo();
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
            //logger.ForContext(context).Information("Entered");
            //contexts.Push(context);
        }

        /// <summary>
        /// Actions on leaving the last grammar node.
        /// </summary>
        /// <param name="isSuccessful">Status of the current node analysis.</param>
        /// <returns>The given node analysis status.</returns>
        private bool Leave(bool isSuccessful)
        {
            // var context = contexts.Pop();
            // var logger = this.logger.ForContext(context);

            // if (isSuccessful)
            // {
            //     logger.Information("+");
            // }
            // else
            // {
            //     logger.Error("-");
            // }

            return isSuccessful;
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        private void MoveNext()
        {
            //logger.ForContext(contexts.Peek()).Information(tokens.CurrentOrLast.Value);
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
                creator.AddLabelForNextRpn(tokens.CurrentOrLast);
                MoveNext();
                if (!tokens.CurrentTokenValueIs(":"))
                {
                    SetError("Colon after label is expected");
                    return Leave(false);
                }

                MoveNext();
                return Leave(Statement());
            }


            if (IfStatement())
            {
                return Leave(true);
            }
            else if (GotoStatement() || Expression())
            {
                if (!tokens.CurrentTokenValueIs(";"))
                {
                    SetError("Semicolon after statement is expected");
                    return Leave(false);
                }

                creator.EndOfStatement();
                creator.Ignore();
                MoveNext();
                return Leave(true);
            }

            return Leave(false);
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

            creator.Indexator(tokens.CurrentOrLast);
            creator.OpenBracket();
            MoveNext();
            if (!Expression())
            {
                SetError("Expression in the indexator is expected");
            }

            if (!tokens.CurrentTokenValueIs("]"))
            {
                SetError("']' in the end of the indexator is expected");
            }

            creator.CloseBracket();
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

            var evalToken = tokens.CurrentOrLast;
            int paramCount = 0;
            creator.OpenBracket();
            MoveNext();

            if (tokens.CurrentTokenValueIs(")"))
            {
                creator.CloseBracket();
                creator.Eval(evalToken, paramCount);
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

            paramCount++;
            while (!tokens.CurrentTokenValueIs(")"))
            {
                if (!tokens.CurrentTokenValueIs(","))
                {
                    SetError("Comma between arguments is expected");
                }

                creator.CloseBracket();
                MoveNext();
                creator.OpenBracket();
                if (!Expression())
                {
                    SetError("Expression in an argument list is expected");
                }

                paramCount++;
            }

            creator.CloseBracket();
            creator.Eval(evalToken, paramCount);
            MoveNext();
            return Leave(true);
        }

        private bool Expression()
        {
            Enter(nameof(Expression));

            if (tokens.CurrentTokenIsUnaryOperation())
            {
                creator.UnaryOperation(tokens.CurrentOrLast);
                MoveNext();
            }

            if (!Operand())
            {
                return Leave(false);
            }

            Tail(); // could or could not be
            while (tokens.CurrentTokenIsBinaryOperation())
            {
                creator.BinaryOperation(tokens.CurrentOrLast);
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression as a binary operation right operand is expected");
                }
            }

            return Leave(true);
        }

        private bool Operand()
        {
            Enter(nameof(Operand));

            if (tokens.CurrentTokenValueIs("("))
            {
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after opening parathesis is expected");
                }

                if (!tokens.CurrentTokenValueIs(")"))
                {
                    SetError("Closing paranthesis in the expression is expected");
                }

                creator.CloseBracket();
                MoveNext();
                return Leave(true);
            }

            return Leave(Literal());
        }

        private bool Literal()
        {
            Enter(nameof(Literal));

            if (tokens.CurrentTokenTypeIs(Token.Type.Integer) ||
                tokens.CurrentTokenTypeIs(Token.Type.Float) ||
                tokens.CurrentTokenTypeIs(Token.Type.String) ||
                tokens.CurrentTokenTypeIs(Token.Type.Identifier))
            {
                creator.Literal(tokens.CurrentOrLast);
                MoveNext();
                return Leave(true);
            }

            return Leave(CodeBlock());
        }

        private bool CodeBlock(bool isPure = true)
        {
            Enter(nameof(CodeBlock));

            if (!tokens.CurrentTokenValueIs("{"))
            {
                return Leave(false);
            }

            if (isPure)
            {
                creator.CodeBlockStart();
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

            if (isPure)
            {
                creator.CodeBlockFinish();
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

            var gotoToken = tokens.CurrentOrLast;
            MoveNext();
            if (!tokens.CurrentTokenTypeIs(Token.Type.Label))
            {
                SetError("Label in the goto statement is expected");
            }

            creator.Label(tokens.CurrentOrLast);
            creator.Goto(gotoToken);
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

            var ifToken = tokens.CurrentOrLast;
            MoveNext();
            if (!Expression())
            {
                return Leave(false);
            }

            creator.If(ifToken);
            if (!CodeBlock(isPure: false) && !Statement())
            {
                SetError("Code block or a single statement in the if-part is expected");
            }

            if (tokens.CurrentTokenValueIs(KeyWords.Else))
            {
                creator.Else(tokens.CurrentOrLast);
                MoveNext();
                if (!CodeBlock(isPure: false) && !Statement())
                {
                    SetError("Code block or a single statement in the else-part is expected");
                }

                creator.EndElse();
            }
            else
            {
                creator.EndIf();
            }

            return Leave(true);
        }
    }
}