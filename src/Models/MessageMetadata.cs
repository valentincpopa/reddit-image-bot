namespace RedditImageBot.Models
{
    public class MessageMetadata
    {
        public int? InternalMessageId { get; private set; }
        public string ExternalMessageId { get; private set; }
        public string ExternalPostId { get; set; }
        public string ExternalCommentId { get; private set; }
    }
}
