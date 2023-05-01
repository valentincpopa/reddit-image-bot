using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Utilities.Exceptions
{
    public class FilterProcessingException : Exception
    {
        public FilterProcessingException(string message) : base(message)
        {
        }
    }
}
