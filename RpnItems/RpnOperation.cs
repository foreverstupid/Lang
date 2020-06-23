namespace Lang.RpnItems
{
    public abstract class RpnOperation : Rpn
    {
        public RpnOperation()
        {
        }

        public RpnOperation(Token token)
            : base(token)
        {
            
        }
    }
}