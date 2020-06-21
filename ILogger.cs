namespace Lang
{
    public interface ILogger
    {
        ILogger ForContext(string context);
        void Information(string message, object[] args = null);
        void Error(string message, object[] args = null);
    }
}