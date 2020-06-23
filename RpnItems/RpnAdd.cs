namespace Lang.RpnItems
{
    public class RpnAdd : RpnOperation
    {
        public RpnAdd(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.AddSubPriority;
    }
}