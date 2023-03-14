namespace RedditImageBot.Models
{
    public class PostMetadata
    {
        public PostMetadata(string externalId, bool isValidImage)
        {
            ExternalId = externalId;
            IsValidImage = isValidImage;
        }

        public PostMetadata(string externalId, string postTitle, string originalImageUrl) : this(externalId, true)
        {
            PostTitle = postTitle;
            OriginalImageUrl = originalImageUrl;
        }

        public int? PostId { get; set; }
        public string ExternalId { get; private set; }
        public bool IsValidImage { get; private set; }
        public string PostTitle { get; private set; }
        public string OriginalImageUrl { get; private set; }
        public string GeneratedImageUrl { get; private set; }
        public bool HasGeneratedImageUrl => !string.IsNullOrWhiteSpace(GeneratedImageUrl);

        public void SetupGeneratedImageUrl(string imageUrl)
        {
            GeneratedImageUrl = imageUrl;
        }

        public void SetupOriginalImageUrl(string imageUrl)
        {
            OriginalImageUrl = imageUrl;
        }
    }
}
