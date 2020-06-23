namespace Lang.RpnItems
{
    public class RpnLess : RpnOperation
    {
        public RpnLess(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.ComparisionPriority;
    }
}