namespace Lang.RpnItems
{
    public class RpnEqual : RpnOperation
    {
        public RpnEqual(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.ComparisionPriority;
    }
}