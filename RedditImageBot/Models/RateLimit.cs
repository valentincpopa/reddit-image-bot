namespace RedditImageBot.Models
{
    public class RateLimit
    {
        public int? RemainingRequests { get; set; }
        public int? UsedRequests { get; set; }
        public int? TimeUntilReset { get; set; }
    }
}
