using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Models
{
    public class RateLimitData
    {
        public int? RemainingRequests { get; set; }
        public int? UsedRequests { get; set; }
        public int? TimeUntilReset { get; set; }
    }
}
