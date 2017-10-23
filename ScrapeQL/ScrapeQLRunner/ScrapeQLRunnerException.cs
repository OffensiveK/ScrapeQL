using System;

namespace ScrapeQLRun
{
    public class ScrapeQLRunnerException : Exception
    {
        public ScrapeQLRunnerException()
        {

        }

        public ScrapeQLRunnerException(string message) : base(message)
        {
        }

        public ScrapeQLRunnerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
