namespace RedditImageBot.Models
{
    public class MessageMetadata
    {
        public int? MessageId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalPostId { get; set; }
        public string ExternalCommentId { get; set; }
    }
}
