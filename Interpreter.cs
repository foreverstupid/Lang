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
        public string Run(ProgramInfo program)
        {
            currentCommand = program.Rpns.First;

            do
            {
                lastCommand = currentCommand;
                currentCommand = currentCommand.Value.Eval(stack, currentCommand);
            }
            while (!(currentCommand is null) && (currentCommand != lastCommand));

            return stack.Count > 0 ? stack.Pop().GetString() : "";
        }
    }
}