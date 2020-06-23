namespace Lang.RpnItems
{
    public class RpnMod : RpnOperation
    {
        public RpnMod(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.MulDivPriority;
    }
}