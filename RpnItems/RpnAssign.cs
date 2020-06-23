namespace Lang.RpnItems
{
    public class RpnAssign : RpnOperation
    {
        public RpnAssign(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.AssignmentPriority;
    }
}