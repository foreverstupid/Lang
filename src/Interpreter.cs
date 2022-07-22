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
        private readonly Stack<RpnConst> stack = new Stack<RpnConst>();

        private LinkedListNode<Rpn> lastCommand;
        private LinkedListNode<Rpn> currentCommand;

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
                        Console.WriteLine("Stack: " + string.Join(" | ", stack));
                        Console.WriteLine($"=> {currentCommand.Value}\n");
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

            try
            {
                return stack.Pop().GetString();
            }
            catch (InterpretationException)
            {
                throw new InterpretationException(
                    "The program result value should be a value that is allowed to cast to string");
            }
        }
    }
}