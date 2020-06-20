namespace Lang
{
    public interface ILogger
    {
        void AddContext(string context);
        void Information(string message, object[] args = null);
        void Error(string message, object[] args = null);
        void CloseLastContext(bool mention);
    }
}