namespace Lang.RpnItems
{
    public class RpnDivide : RpnOperation
    {
        public RpnDivide(Token token)
            : base(token)
        {
            
        }

        protected override int Priority => RpnOperation.MulDivPriority;
    }
}