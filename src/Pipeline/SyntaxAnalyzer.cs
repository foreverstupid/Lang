using System.Collections.Generic;
using Lang.Content;
using Lang.Exceptions;
using Lang.Logging;
using Lang.RpnItems;

namespace Lang.Pipeline
{
    /// <summary>
    /// Performs syntax analysis and preliminary interpretation info cration.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private readonly ILogger logger;
        private readonly ProgramCreator creator = new ProgramCreator();

        private TreeView treeView;
        private TokenEnumerator tokens;
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
            treeView = new TreeView(logger);

            try
            {
                Expression();
            }
            catch (RpnCreationException e)
            {
                SetError(e.Message);
            }

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
                treeView.AddNode(context + $" [{t.Value} ({t.Line}:{t.StartPosition})]");
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
                if (isSuccessful)
                {
                    treeView.Commit();
                }
                else
                {
                    treeView.RejectLastNode();
                }
            }

            return isSuccessful;
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        private void MoveNext()
        {
            tokens.MoveNext();
        }

        // Grammar nodes handlers

        private bool Expression()
        {
            Enter(nameof(Expression));

            if (Group() || Jump())
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
                        $"'{unaryOperationToken.Value}' is expected");
                }

                return Leave(false);
            }

            Tail();         // could or could not be
            while (Tie())
            {
            }

            return Leave(true);
        }

        private bool Tie()
        {
            Enter(nameof(Tie));

            bool isReversed = false;
            if (tokens.CurrentTokenIsSeparator(Operations.Not))
            {
                isReversed = true;
                MoveNext();
            }

            if (tokens.CurrentTokenIsBinaryOperation())
            {
                var operation = tokens.CurrentOrLast;
                MoveNext();

                if (tokens.CurrentTokenIsSeparator(Operations.Assign) ||
                    tokens.CurrentTokenIsSeparator(Operations.Insert))
                {
                    creator.Assignment(tokens.CurrentOrLast, operation, isReversed);
                    MoveNext();
                }
                else
                {
                    creator.BinaryOperation(operation, isReversed);
                }

                CreateRightExpression(tokens.CurrentOrLast);
            }
            else
            {
                if (!isReversed)
                {
                    return Leave(false);
                }

                SetError("Binary operation is expected");
            }

            return Leave(true);

            void CreateRightExpression(Token token)
            {
                if (!Expression())
                {
                    SetError(
                        "Expression as a binary operation " +
                        $"'{token.Value}' right operand is expected");
                }
            }
        }

        private bool Group()
        {
            Enter(nameof(Group));

            if (!tokens.CurrentTokenIsSeparator(Syntax.GroupStart))
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
            while (tokens.CurrentTokenIsSeparator(Syntax.GroupDelimiter))
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

            if (!tokens.CurrentTokenIsSeparator(Syntax.GroupEnd))
            {
                SetError($"'{Syntax.GroupEnd}' is expected at the end of the expression group");
            }

            MoveNext();
            return Leave(true);
        }

        private bool Jump()
        {
            Enter(nameof(Jump));

            if (tokens.CurrentTokenIsSeparator(KeyWords.Break))
            {
                creator.CycleBreak();
                MoveNext();
                return Leave(true);
            }

            if (tokens.CurrentTokenIsSeparator(KeyWords.Continue))
            {
                creator.CycleContinue();
                MoveNext();
                return Leave(true);
            }

            if (tokens.CurrentTokenIsSeparator(KeyWords.Return))
            {
                MoveNext();
                if (!Expression())
                {
                    throw new SyntaxException(
                        "Expected expression in the lambda break operation");
                }

                creator.LambdaReturn();
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

            if (tokens.CurrentTokenIsSeparator(Syntax.IndexatorStart))
            {
                creator.Indexator(tokens.CurrentOrLast);
                creator.OpenBracket();
                MoveNext();

                if (!Expression())
                {
                    SetError("Expression in the indexator is expected");
                }

                if (!tokens.CurrentTokenIsSeparator(Syntax.IndexatorEnd))
                {
                    SetError($"'{Syntax.IndexatorEnd}' at the end of the indexator is expected");
                }

                creator.CloseBracket();
                MoveNext();
                return Leave(true);
            }

            if (tokens.CurrentTokenIsSeparator(Syntax.FieldSeparator))
            {
                creator.Indexator(tokens.CurrentOrLast);
                MoveNext();

                if (!tokens.CurrentTokenTypeIs(Token.Type.String))
                {
                    SetError("Pseudo-field name identifier is expected");
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

            if (!tokens.CurrentTokenIsSeparator(Syntax.EvalArgsStart))
            {
                return Leave(false);
            }

            var evalToken = tokens.CurrentOrLast;
            int paramCount = 0;
            creator.EvalStart();
            creator.OpenBracket();
            MoveNext();

            if (tokens.CurrentTokenIsSeparator(Syntax.EvalArgsEnd))
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
            while (!tokens.CurrentTokenIsSeparator(Syntax.EvalArgsEnd))
            {
                if (!tokens.CurrentTokenIsSeparator(Syntax.EvalArgsSeparator))
                {
                    SetError($"'{Syntax.EvalArgsSeparator}' between arguments is expected");
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

            if (tokens.CurrentTokenIsSeparator(Syntax.ExpressionBracketOpen))
            {
                creator.OpenBracket();
                MoveNext();
                if (!Expression())
                {
                    SetError("Expression after an opening paranthesis is expected");
                }

                if (!tokens.CurrentTokenIsSeparator(Syntax.ExpressionBracketClose))
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
            if (!tokens.CurrentTokenTypeIs(Token.Type.Identifier) &&
                !tokens.CurrentTokenIsSeparator(KeyWords.Let) &&
                !tokens.CurrentTokenIsSeparator(KeyWords.Ref))
            {
                return Leave(false);
            }

            creator.OpenBracket();
            if (tokens.CurrentTokenIsSeparator(KeyWords.Let))
            {
                MoveNext();

                bool isRef = false;
                if (tokens.CurrentTokenIsSeparator(KeyWords.Ref))
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
            else if (tokens.CurrentTokenIsSeparator(KeyWords.Ref))
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

            if (!tokens.CurrentTokenIsSeparator(Syntax.ParamsApply))
            {
                SetError($"'{Syntax.ParamsApply}' after lambda parameter list is expected");
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

            if (!tokens.CurrentTokenIsSeparator(Syntax.ParamsStart))
            {
                return Leave(false);
            }

            MoveNext();
            creator.LambdaStart();

            if (tokens.CurrentTokenIsSeparator(Syntax.ParamsEnd))
            {
                MoveNext();
                return Leave(true);
            }

            Parameter();
            while (!tokens.CurrentTokenIsSeparator(Syntax.ParamsEnd))
            {
                if (!tokens.CurrentTokenIsSeparator(Syntax.ParamsSeparator))
                {
                    SetError(
                        $"'{Syntax.ParamsSeparator}' in parameter enumeration is expected");
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
            if (tokens.CurrentTokenIsSeparator(KeyWords.Ref))
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

            if (!tokens.CurrentTokenIsSeparator(KeyWords.If))
            {
                return Leave(false);
            }

            var ifToken = tokens.CurrentOrLast;
            creator.IfStart();
            MoveNext();

            if (!tokens.CurrentTokenIsSeparator(Syntax.ConditionStart))
            {
                SetError("Openning paranthesis at the beggining of the condition is expected");
            }

            creator.OpenBracket();
            MoveNext();
            if (!Expression())
            {
                SetError("Invalid condition expression");
            }

            if (!tokens.CurrentTokenIsSeparator(Syntax.ConditionEnd))
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
            if (tokens.CurrentTokenIsSeparator(KeyWords.Else))
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

            if (!tokens.CurrentTokenIsSeparator(KeyWords.While))
            {
                return Leave(false);
            }

            creator.CycleStart();
            var whileToken = tokens.CurrentOrLast;
            MoveNext();

            if (!tokens.CurrentTokenIsSeparator(Syntax.ConditionStart))
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

            if (!tokens.CurrentTokenIsSeparator(Syntax.ConditionEnd))
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
            if (!tokens.CurrentTokenIsSeparator(Syntax.InitializerStart))
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

            while (tokens.CurrentTokenIsSeparator(Syntax.InitializerSeparator))
            {
                MoveNext();
                creator.OpenBracket();
                if (!InitAtom(variableToken, ref valueIdx))
                {
                    SetError(
                        $"Expected initializer item after '{Syntax.InitializerSeparator}'");
                }

                creator.CloseBracket();
                creator.Ignore(tokens.CurrentOrLast);
            }

            if (!tokens.CurrentTokenIsSeparator(Syntax.InitializerEnd))
            {
                SetError(
                    $"'{Syntax.InitializerEnd}' at the end of an initializer expected");
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
            creator.BinaryOperation(
                new Token(Operations.Assign, Token.Type.Separator, t.StartPosition, t.Line));

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
                if (!tokens.CurrentTokenIsSeparator(Operations.Assign))
                {
                    SetError(
                        $"'{Operations.Assign}' is expected in the initializer item");
                }

                creator.BinaryOperation(tokens.CurrentOrLast);
                MoveNext();
                creator.OpenBracket();
                if (!Expression())
                {
                    SetError(
                        $"Expected assigning expression after '{Operations.Assign}' " +
                        "in the initializer item");
                }

                creator.CloseBracket();
            }
        }
    }
}