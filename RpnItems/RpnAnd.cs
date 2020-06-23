namespace Lang.RpnItems
{
    public class RpnAnd : RpnOperation
    {
        public RpnAnd(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.LogicalOperationPriority;
    }
}