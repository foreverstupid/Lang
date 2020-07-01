using System;
using System.Collections.Generic;
using Lang.RpnItems;

namespace Lang
{
    /// <summary>
    /// Performs the program.
    /// </summary>
    public class Interpreter
    {
        private LinkedListNode<Rpn> lastCommand;
        private LinkedListNode<Rpn> currentCommand;
        private Stack<RpnConst> stack = new Stack<RpnConst>();

        /// <summary>
        /// Runs the given program.
        /// </summary>
        /// <param name="program">The program to run.</param>
        /// <returns>The value of the last running statement in a string format.</returns>
        public string Run(LinkedList<Rpn> program, bool isDebug = false)
        {
            currentCommand = program.First;

            if (isDebug)
            {
                Console.WriteLine("==================== INTERPRETATION ====================");
            }

            do
            {
                lastCommand = currentCommand;
                try
                {
                    if (isDebug)
                    {
                        var positionInfo =
                            currentCommand.Value.Token is null
                            ? ""
                            : $" ({currentCommand.Value.Token.Line}:{currentCommand.Value.Token.StartPosition})";

                        Console.WriteLine("Stack: " + string.Join(" | ", stack));
                        Console.WriteLine("=> " + currentCommand.Value + positionInfo + "\n");
                    }

                    currentCommand = currentCommand.Value.Eval(stack, currentCommand);
                }
                catch (InterpretationException e)
                {
                    Console.WriteLine(
                        $"({currentCommand.Value.Token.Line}:{currentCommand.Value.Token.StartPosition}) " +
                        e.Message
                    );

                    return "ERROR";
                }
            }
            while (!(currentCommand is null) && (currentCommand != lastCommand));

            return stack.Pop().GetString();
        }
    }
}