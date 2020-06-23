namespace Lang.RpnItems
{
    public abstract class RpnOperation : Rpn
    {
        protected const int IndexatorPriority = 1000;
        protected const int DereferencePriority = 999;
        protected const int UnarOperationPriority = 900;
        protected const int ComparisionPriority = 800;
        protected const int LogicalOperationPriority = 700;
        protected const int MulDivPriority = 600;
        protected const int AddSubPriority = 500;
        protected const int AssignmentPriority = 200;

        public RpnOperation(Token token)
            : base(token)
        {

        }

        protected abstract int Priority { get; }

        public bool HasLessPriorityThan(RpnOperation anotherOperation)
            => this.Priority < anotherOperation.Priority;
    }
}