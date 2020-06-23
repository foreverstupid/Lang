namespace Lang.RpnItems
{
    public class RpnIndexator : RpnOperation
    {
        public RpnIndexator(Token token)
            : base(token)
        {
        }

        protected override int Priority => 1000;
    }
}