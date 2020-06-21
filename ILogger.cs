namespace Lang
{
    public interface ILogger
    {
        ILogger ForContext(string context);
        void Information(string message, params object[] args);
        void Error(string message, params object[] args);
    }
}