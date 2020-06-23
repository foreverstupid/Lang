namespace Lang.RpnItems
{
    public class RpnMultiply : RpnOperation
    {
        public RpnMultiply(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.MulDivPriority;
    }
}