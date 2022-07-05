namespace Lang
{
    public class ConsoleLogger : ILogger
    {
        private readonly string context;
        private readonly bool omitAdditionalInfo;

        public ConsoleLogger(string context = "Lang", bool omitAdditionalInfo = false)
        {
            this.context = context;
            this.omitAdditionalInfo = omitAdditionalInfo;
        }

        public ILogger ForContext(string context)
        {
            return new ConsoleLogger(this.context + "/" + context);
        }

        public void Error(string message, params object[] args)
        {
            message = ConstructMessage(message, args, error: true);
            System.Console.WriteLine(message);
        }

        public void Information(string message, params object[] args)
        {
            message = ConstructMessage(message, args);
            System.Console.WriteLine(message);
        }

        private string ConstructMessage(string message, object[] args, bool error = false)
        {
            if (args.Length > 0)
            {
                message = string.Format(message, args);
            }

            if (omitAdditionalInfo)
            {
                return message;
            }

            return
                error
                ? $"[{context}] ERR: {message}"
                : $"[{context}] INF: {message}";
        }
    }
}