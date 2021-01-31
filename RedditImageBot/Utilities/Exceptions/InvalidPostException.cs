using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Utilities.Exceptions
{
    public class InvalidPostException : Exception
    {
        public InvalidPostException(string message) : base(message)
        {
        }
    }
}
