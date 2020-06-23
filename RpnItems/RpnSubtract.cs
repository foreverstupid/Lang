namespace Lang.RpnItems
{
    public class RpnSubtract : RpnOperation
    {
        public RpnSubtract(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.AddSubPriority;
    }
}