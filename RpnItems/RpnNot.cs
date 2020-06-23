namespace Lang.RpnItems
{
    public class RpnNot : RpnOperation
    {
        public RpnNot(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.UnarOperationPriority;
    }
}