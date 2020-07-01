using System.Collections.Generic;
using Lang.RpnItems;

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
        private ProgramCreator creator = new ProgramCreator();

        private bool isDebug;

        public SyntaxAnalyzer(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Performs analysis, creating needed interpratation info.
        /// </summary>
        /// <param name="tokens">The collection of the code tokens.</param>
        /// <returns>Needed information for interpretation.</returns>
        public LinkedList<Rpn> Analyse(IEnumerable<Token> tokens, bool isDebug = false)
        {
            this.tokens = new TokenEnumerator(tokens);
            this.isDebug = isDebug;

            creator.StartProgramCreation();
            contexts.Clear();

            Expression();

            if (!this.tokens.IsFinished)
            {
                SetError("Unknown statement");
            }

            return creator.FinishProgramCreation();
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
            if (isDebug)
            {
                logger.ForContext(context).Information("Entered");
                contexts.Push(context);
            }
        }

        /// <summary>
        /// Actions on leaving the last grammar node.
        /// </summary>
        /// <param name="isSuccessful">Status of the current node analysis.</param>
        /// <returns>The given node analysis status.</returns>
        private bool Leave(bool isSuccessful)
        {
            if (isDebug)
            {
                var context = contexts.Pop();
                var logger = this.logger.ForContext(context);

                if (isSuccessful)
                {
                    logger.Information("+");
                }
                else
                {
                    logger.Error("-");
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        private void MoveNext()
        {
            if (isDebug)
            {
                logger.ForContext(contexts.Peek()).Information(tokens.CurrentOrLast.Value);
            }

            tokens.MoveNext();
        }

        // Grammar nodes handlers

        private bool Expression()
        {
            Enter(nameof(Expression));

            if (Group())
            {
                return Leave(true);
            }

            Token unaryOperationToken = null;
            if (tokens.CurrentTokenIsUnaryOperation())
            {
                creator.UnaryOperation(tokens.CurrentOrLast);
                unaryOperationToken = tokens.CurrentOrLast;
                MoveNext();
            }

            if (!Operand())
            {
                if (!(unaryOperationToken is null))
                {
                    SetError(
                        "Operand of the unary operation " +
                        $"'{unaryOperationToken.Value}' is expected"
                    );
                }

                return Leave(false);
            }

            Tail(); // could or could not be
            while (tokens.CurrentTokenIsBinaryOperation())
            {
                creator.BinaryOperation(tokens.CurrentOrLast);
                var binaryOperationToken = tokens.CurrentOrLast;
                MoveNext();

                if (!Expression())
                {
                    SetError(
                        "Expression as a binary operation " +
                        $"'{binaryOperationToken.Value}' right operand is expected"
                    );
                }
            }

            return Leave(true);
        }

        private bool Group()
        {
            Enter(nameof(Group));

            if (!tokens.CurrentTokenValueIs("{"))
            {
                return Leave(false);
            }

            creator.OpenBracket();
            MoveNext();
            if (!Expression())
            {
                SetError("Expression group should have at least one expression");
            }

            creator.CloseBracket();
            while (tokens.CurrentTokenValueIs(";"))
            {
                creator.Ignore(tokens.CurrentOrLast);
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Invalid expression");
                }

                creator.CloseBracket();
            }

            if (!tokens.CurrentTokenValueIs("}"))
            {
                SetError("'}' is expected at the end of the expression group");
            }

            MoveNext();
            return Leave(true);
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
                return Leave(false);
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
                SetError("']' at the end of the indexator is expected");
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

        private bool Operand()
        {
            Enter(nameof(Operand));

            if (tokens.CurrentTokenValueIs("("))
            {
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after an opening paranthesis is expected");
                }

                if (!tokens.CurrentTokenValueIs(")"))
                {
                    SetError("Closing paranthesis in the expression is expected");
                }

                creator.CloseBracket();
                MoveNext();
                return Leave(true);
            }

            return Leave(IfExpression() || WhileExpression() || Literal() || Lambda());
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

            return Leave(false);
        }

        private bool Lambda()
        {
            Enter(nameof(Lambda));

            if (!Parameters())
            {
                return Leave(false);
            }

            if (!tokens.CurrentTokenValueIs("=>"))
            {
                SetError("'=>' after lambda parameter list is expected");
            }

            MoveNext();

            if (!Expression())
            {
                SetError("Invalid lambda body");
            }

            creator.LambdaFinish();
            return Leave(true);
        }

        private bool Parameters()
        {
            Enter(nameof(Parameters));

            if (!tokens.CurrentTokenValueIs("["))
            {
                return Leave(false);
            }

            MoveNext();
            creator.LambdaStart();

            if (tokens.CurrentTokenValueIs("]"))
            {
                MoveNext();
                return Leave(true);
            }

            if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
            {
                SetError("Parameter definition is expected");
            }

            creator.Parameter(tokens.CurrentOrLast);
            MoveNext();
            while (!tokens.CurrentTokenValueIs("]"))
            {
                if (!tokens.CurrentTokenValueIs(","))
                {
                    SetError("Comma in parameter enumeration is expected");
                }

                MoveNext();
                if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
                {
                    SetError("Parameter definition is expected");
                }

                creator.Parameter(tokens.CurrentOrLast);
                MoveNext();
            }

            MoveNext();
            return Leave(true);
        }

        private bool IfExpression()
        {
            Enter(nameof(IfExpression));

            if (!tokens.CurrentTokenValueIs(KeyWords.If))
            {
                return Leave(false);
            }

            var ifToken = tokens.CurrentOrLast;
            creator.IfStart();
            MoveNext();

            if (!tokens.CurrentTokenValueIs("("))
            {
                SetError("Openning paranthesis at the beggining of the condition is expected");
            }

            creator.OpenBracket();
            MoveNext();
            if (!Expression())
            {
                SetError("Invalid condition expression");
            }

            if (!tokens.CurrentTokenValueIs(")"))
            {
                SetError("Closing paranthesis at the end of the condition is expected");
            }

            creator.CloseBracket();
            MoveNext();

            creator.If(ifToken);
            creator.OpenBracket();
            if (!Expression())
            {
                SetError("Invalid if-part body");
            }

            creator.CloseBracket();
            if (tokens.CurrentTokenValueIs(KeyWords.Else))
            {
                creator.Else(tokens.CurrentOrLast);
                MoveNext();
                creator.OpenBracket();

                if (!Expression())
                {
                    SetError("Invalid else-part body");
                }

                creator.CloseBracket();
                creator.EndElse();
            }
            else
            {
                creator.EndIf();
            }

            return Leave(true);
        }

        private bool WhileExpression()
        {
            Enter(nameof(WhileExpression));

            if (!tokens.CurrentTokenValueIs(KeyWords.While))
            {
                return Leave(false);
            }

            creator.CycleStart();
            var whileToken = tokens.CurrentOrLast;
            MoveNext();

            if (!tokens.CurrentTokenValueIs("("))
            {
                SetError(
                    "Opening paranthesis at the beginning " +
                    "of the cycle condition is expected"
                );
            }

            creator.OpenBracket();
            MoveNext();

            if (!Expression())
            {
                SetError("Invalid condition expression");
            }

            if (!tokens.CurrentTokenValueIs(")"))
            {
                SetError(
                    "Closing paranthesis at the end " +
                    "of the cycle condition is expected"
                );
            }

            creator.CloseBracket();
            MoveNext();
            creator.While(whileToken);
            creator.OpenBracket();

            if (!Expression())
            {
                SetError("Invalid cycle body");
            }

            creator.CloseBracket();
            creator.CycleEnd();
            return Leave(true);
        }
    }
}