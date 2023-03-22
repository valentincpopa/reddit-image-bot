using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Utilities.Exceptions
{
    public class MetadataException : Exception
    {
        public MetadataException(string message) : base(message)
        {
        }
    }
}
