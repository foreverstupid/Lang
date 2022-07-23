using System.Collections.Generic;
using Lang.Pipeline;

namespace Lang.RpnItems
{
    /// <summary>
    /// RPN item that performs some operation over the program stack elements,
    /// returning a single value to the stack.
    /// </summary>
    public abstract class RpnOperation : RpnSuccessive
    {
        protected const int IndexatorPriority = 1000;
        protected const int DereferencePriority = 999;
        protected const int UnarOperationPriority = 900;
        protected const int MulDivPriority = 600;
        protected const int AddSubPriority = 500;
        protected const int ShiftPriority = 450;
        protected const int ComparisionPriority = 400;
        protected const int CastPriority = 300;
        protected const int LogicalOperationPriority = 250;
        protected const int AssignmentPriority = 200;

        public RpnOperation(Token token)
            : base(token)
        {

        }

        /// <summary>
        /// The operation priority.
        /// </summary>
        protected abstract int Priority { get; }

        /// <summary>
        /// Checks whether the current operation has the less priority than the
        /// given one or not.
        /// </summary>
        public virtual bool HasLessPriorityThan(RpnOperation anotherOperation)
        {
            return this.Priority < anotherOperation.Priority;
        }

        /// <inheritdoc/>
        protected override sealed void EvalCore(Stack<RpnConst> stack)
            => stack.Push(GetResult(stack));

        /// <summary>
        /// Performs calculations returning the result of operation.
        /// </summary>
        protected abstract RpnConst GetResult(Stack<RpnConst> stack);
    }
}