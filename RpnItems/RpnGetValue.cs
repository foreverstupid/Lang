namespace Lang.RpnItems
{
    public class RpnGetValue : RpnOperation
    {
        public RpnGetValue(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.DereferencePriority;
    }
}