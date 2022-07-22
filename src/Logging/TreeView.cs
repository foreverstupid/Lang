using System.Linq;
using System.Collections.Generic;

namespace Lang.Logging
{
    /// <summary>
    /// Help class that constructs a tree-view representation of the program syntax.
    /// </summary>
    public class TreeView
    {
        private static readonly string Shift = new string(' ', 2);

        private readonly ILogger logger;
        private readonly Stack<string> uncommitedNodes = new Stack<string>();
        private readonly Stack<string> commitedNodes = new Stack<string>();

        private int indent = 0;
        private bool isLastPrintedClosing = false;

        public TreeView(ILogger logger)
        {
            this.logger = logger;
        }

        public void AddNode(string node)
        {
            uncommitedNodes.Push(node);
            indent++;
        }

        public void RejectLastNode()
        {
            uncommitedNodes.TryPop(out _);
            DecIndent();
        }

        public void Commit()
        {
            if (uncommitedNodes.Count > 0)
            {
                int tmpIndent = commitedNodes.Count;
                foreach (var node in uncommitedNodes.Reverse())
                {
                    Print(node, tmpIndent);
                    tmpIndent++;
                    commitedNodes.Push(node);
                }

                uncommitedNodes.Clear();
            }

            var last = commitedNodes.Pop();
            DecIndent();
            Print(last, indent, closing: true);
        }

        private void Print(string node, int indent, bool closing = false)
        {
            var shift = indent == 0 ? "" : Enumerable.Repeat(Shift, indent).Aggregate((v, s) => v + s);
            var tag = closing ? $"</{node}>" : $"<{node}>";

            if (!closing && isLastPrintedClosing)
            {
                logger.Information("");
            }

            logger.Information(shift + tag);
            isLastPrintedClosing = closing;
        }

        private void DecIndent()
        {
            indent--;
            if (indent < 0)
            {
                indent = 0;
            }
        }
    }
}