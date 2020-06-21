namespace Lang
{
    public class ConsoleLogger : ILogger
    {
        private readonly string context;

        public ConsoleLogger(string context = "Lang")
        {
            this.context = context;
        }

        public ILogger ForContext(string context)
        {
            return new ConsoleLogger(this.context + "/" + context);
        }

        public void Error(string message, object[] args = null)
        {
            System.Console.WriteLine($"{context} - [ERROR]: {message}", args);
        }

        public void Information(string message, object[] args = null)
        {
            System.Console.WriteLine($"{context} - [Info]: {message}", args);
        }
    }
}