namespace Lang.RpnItems
{
    public class RpnGreater : RpnOperation
    {
        public RpnGreater(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.ComparisionPriority;
    }
}