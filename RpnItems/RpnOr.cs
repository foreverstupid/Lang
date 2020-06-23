namespace Lang.RpnItems
{
    public class RpnOr : RpnOperation
    {
        public RpnOr(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.LogicalOperationPriority;
    }
}