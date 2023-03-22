using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Utilities.Exceptions
{
    public class PipelineException : Exception
    {
        public PipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
