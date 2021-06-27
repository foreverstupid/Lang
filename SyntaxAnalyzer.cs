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
                var t = tokens.CurrentOrLast;
                logger.ForContext(context).Information($"Entered ({t.Line}:{t.StartPosition})");
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
                logger.ForContext(contexts.Peek()).Information(tokens.CurrentOrLast.ToString());
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
            while (tokens.CurrentTokenIsUnaryOperation())
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

            if (!tokens.CurrentTokenIsSeparator("{"))
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
            while (tokens.CurrentTokenIsSeparator(";"))
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

            if (!tokens.CurrentTokenIsSeparator("}"))
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

            if (tokens.CurrentTokenIsSeparator("["))
            {
                creator.Indexator(tokens.CurrentOrLast);
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression in the indexator is expected");
                }

                if (!tokens.CurrentTokenIsSeparator("]"))
                {
                    SetError("']' at the end of the indexator is expected");
                }

                creator.CloseBracket();
                MoveNext();
                return Leave(true);
            }

            if (tokens.CurrentTokenIsSeparator("."))
            {
                creator.Indexator(tokens.CurrentOrLast);
                MoveNext();
                if (!tokens.CurrentTokenTypeIs(Token.Type.String))
                {
                    SetError("Pseudo-field name identifier is expected after dot");
                }

                creator.Literal(tokens.CurrentOrLast);
                MoveNext();
                return Leave(true);
            }

            return Leave(false);
        }

        private bool Arguments()
        {
            Enter(nameof(Arguments));

            if (!tokens.CurrentTokenIsSeparator("("))
            {
                return Leave(false);
            }

            var evalToken = tokens.CurrentOrLast;
            int paramCount = 0;
            creator.OpenBracket();
            MoveNext();

            if (tokens.CurrentTokenIsSeparator(")"))
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
            while (!tokens.CurrentTokenIsSeparator(")"))
            {
                if (!tokens.CurrentTokenIsSeparator(","))
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

            if (tokens.CurrentTokenIsSeparator("("))
            {
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after an opening paranthesis is expected");
                }

                if (!tokens.CurrentTokenIsSeparator(")"))
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
                tokens.CurrentTokenTypeIs(Token.Type.String))
            {
                creator.Literal(tokens.CurrentOrLast);
                MoveNext();
                return Leave(true);
            }

            return Leave(Variable());
        }

        private bool Variable()
        {
            Enter(nameof(Variable));

            Token variableToken = null;
            if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
            {
                return Leave(false);
            }

            creator.OpenBracket();
            if (tokens.CurrentTokenValueIs(KeyWords.Let))
            {
                MoveNext();

                bool isRef = false;
                if (tokens.CurrentTokenTypeIs(Token.Type.Identifier) &&
                    tokens.CurrentTokenValueIs(KeyWords.Ref))
                {
                    isRef = true;
                    MoveNext();
                }

                if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
                {
                    SetError(
                        "Variable is expected after " +
                        $"'{(isRef ? KeyWords.Ref : KeyWords.Let)}' keyword");
                }

                variableToken = tokens.CurrentOrLast;
                creator.LocalVariable(tokens.CurrentOrLast, isRef);
                MoveNext();
            }
            else if (tokens.CurrentTokenValueIs(KeyWords.Ref))
            {
                MoveNext();
                if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
                {
                    SetError($"Variable name is expected after '{KeyWords.Ref}' keyword");
                }

                variableToken = tokens.CurrentOrLast;
                creator.GlobalRefVariable(tokens.CurrentOrLast);
                MoveNext();
            }
            else
            {
                variableToken = tokens.CurrentOrLast;
                creator.Literal(tokens.CurrentOrLast);
                MoveNext();
            }

            Initializer(variableToken);      // can or can not be
            creator.CloseBracket();
            return Leave(true);
        }

        private bool Lambda()
        {
            Enter(nameof(Lambda));

            if (!Parameters())
            {
                return Leave(false);
            }

            if (!tokens.CurrentTokenIsSeparator("=>"))
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

            if (!tokens.CurrentTokenIsSeparator("["))
            {
                return Leave(false);
            }

            MoveNext();
            creator.LambdaStart();

            if (tokens.CurrentTokenIsSeparator("]"))
            {
                MoveNext();
                return Leave(true);
            }

            Parameter();
            while (!tokens.CurrentTokenIsSeparator("]"))
            {
                if (!tokens.CurrentTokenIsSeparator(","))
                {
                    SetError("Comma in parameter enumeration is expected");
                }

                MoveNext();
                Parameter();
            }

            MoveNext();
            return Leave(true);
        }

        private bool Parameter()
        {
            bool isRef = false;
            if (tokens.CurrentTokenTypeIs(Token.Type.Identifier) &&
                tokens.CurrentTokenValueIs(KeyWords.Ref))
            {
                isRef = true;
                MoveNext();
            }

            if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier))
            {
                SetError("Parameter definition is expected");
            }

            creator.Parameter(tokens.CurrentOrLast, isRef);
            MoveNext();

            return true;
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

            if (!tokens.CurrentTokenIsSeparator("("))
            {
                SetError("Openning paranthesis at the beggining of the condition is expected");
            }

            creator.OpenBracket();
            MoveNext();
            if (!Expression())
            {
                SetError("Invalid condition expression");
            }

            if (!tokens.CurrentTokenIsSeparator(")"))
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

            if (!tokens.CurrentTokenIsSeparator("("))
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

            if (!tokens.CurrentTokenIsSeparator(")"))
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

        private bool Initializer(Token variableToken)
        {
            Enter(nameof(Initializer));

            int valueIdx = 0;
            if (!tokens.CurrentTokenIsSeparator("{"))
            {
                return Leave(false);
            }

            MoveNext();
            creator.OpenBracket();
            if (!InitAtom(variableToken, ref valueIdx))
            {
                SetError("Expected at least one initializer item");
            }

            creator.Ignore(tokens.CurrentOrLast);
            creator.CloseBracket();

            while (tokens.CurrentTokenIsSeparator(","))
            {
                MoveNext();
                creator.OpenBracket();
                if (!InitAtom(variableToken, ref valueIdx))
                {
                    SetError("Expected initializer item after a comma");
                }

                creator.CloseBracket();
                creator.Ignore(tokens.CurrentOrLast);
            }

            if (!tokens.CurrentTokenIsSeparator("}"))
            {
                SetError("'}' at the end of an initializer expected");
            }

            MoveNext();
            return Leave(true);
        }

        private bool InitAtom(Token variableToken, ref int idx)
        {
            Enter(nameof(InitAtom));

            creator.OpenBracket();
            creator.Literal(variableToken);

            if (Indexator())
            {
                while (Indexator())
                {
                }

                InitAtomTail();
                creator.CloseBracket();
                return Leave(true);
            }

            var t = tokens.CurrentOrLast;
            creator.Indexator(t);
            creator.Literal(new Token(idx.ToString(), Token.Type.Integer, t.StartPosition, t.Line));
            creator.BinaryOperation(new Token("=", Token.Type.Separator, t.StartPosition, t.Line));
            creator.OpenBracket();

            if (!Expression())
            {
                SetError("Expression expected as the initializer item");
            }

            idx++;
            creator.CloseBracket();
            creator.CloseBracket();

            return Leave(true);

            void InitAtomTail()
            {
                if (!tokens.CurrentTokenIsSeparator("="))
                {
                    SetError("'=' is expected in the initializer item");
                }

                creator.BinaryOperation(tokens.CurrentOrLast);
                MoveNext();
                creator.OpenBracket();
                if (!Expression())
                {
                    SetError("Expected assigning expression after '=' in the initializer item");
                }

                creator.CloseBracket();
            }
        }
    }
}