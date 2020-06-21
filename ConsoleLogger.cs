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

        public void Error(string message, params object[] args)
        {
            System.Console.WriteLine(context + " - [ERROR]: " + message);
        }

        public void Information(string message, params object[] args)
        {
            System.Console.WriteLine(context + " - [Info]: " + message);
        }
    }
}