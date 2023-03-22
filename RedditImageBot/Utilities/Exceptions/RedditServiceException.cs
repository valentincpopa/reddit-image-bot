using System;

namespace RedditImageBot.Utilities.Exceptions
{
    internal class RedditServiceException : Exception
    {
        public RedditServiceException(string message) : base(message)
        {
        }
    }
}
