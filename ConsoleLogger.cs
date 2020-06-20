using System.Collections.Generic;

namespace Lang
{
    public class ConsoleLogger : ILogger
    {
        private List<string> contexts = new List<string>();
        public void AddContext(string context)
        {
            contexts.Add(context);
        }

        public void CloseLastContext(bool mention)
        {
            try
            {
                if (mention)
                {
                    System.Console.WriteLine(contexts[^1]);
                }

                contexts.RemoveAt(contexts.Count - 1);
            }
            catch{}
        }

        public void Error(string message, object[] args = null)
        {
            throw new System.NotImplementedException();
        }

        public void Information(string message, object[] args = null)
        {
            throw new System.NotImplementedException();
        }
    }
}