namespace Lang.RpnItems
{
    public class RpnNegate : RpnOperation
    {
        public RpnNegate(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.UnarOperationPriority;
    }
}