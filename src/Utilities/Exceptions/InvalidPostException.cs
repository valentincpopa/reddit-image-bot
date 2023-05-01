using System;

namespace RedditImageBot.Utilities.Exceptions
{
    public class InvalidPostException : Exception
    {
        public InvalidPostException(string message) : base(message)
        {
        }
    }
}
