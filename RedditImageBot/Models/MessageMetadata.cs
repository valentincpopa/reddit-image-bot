namespace RedditImageBot.Models
{
    public class MessageMetadata
    {
        public int? MessageId { get; private set; }
        public string ExternalId { get; private set; }
        public string ExternalPostId { get; set; }
        public string ExternalCommentId { get; private set; }
    }
}
