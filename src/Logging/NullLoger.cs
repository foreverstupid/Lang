namespace Lang.Logging
{
    /// <summary>
    /// Logger that doesn't log.
    /// </summary>
    public class NullLogger : ILogger
    {
        public void Error(string message, params object[] args)
        {
        }

        public ILogger ForContext(string context)
        {
            return new NullLogger();
        }

        public void Information(string message, params object[] args)
        {
        }
    }
}