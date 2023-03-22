using System;

namespace RedditImageBot.Models
{
    public class PostMetadata
    {
        public PostMetadata(string externalId, bool isValidImage)
        {
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentException($"'{nameof(externalId)}' cannot be null or whitespace.", nameof(externalId));
            }

            ExternalId = externalId;
            IsValidImage = isValidImage;
        }

        public PostMetadata(string externalId, string postTitle, string originalImageUrl) : this(externalId, true)
        {
            if (string.IsNullOrWhiteSpace(postTitle))
            {
                throw new ArgumentException($"'{nameof(postTitle)}' cannot be null or whitespace.", nameof(postTitle));
            }

            if (string.IsNullOrWhiteSpace(originalImageUrl))
            {
                throw new ArgumentException($"'{nameof(originalImageUrl)}' cannot be null or whitespace.", nameof(originalImageUrl));
            }

            PostTitle = postTitle;
            OriginalImageUrl = originalImageUrl;
        }

        public int? PostId { get; set; }
        public string ExternalId { get; private set; }
        public bool IsValidImage { get; private set; }
        public string PostTitle { get; private set; }
        public string OriginalImageUrl { get; private set; }
        public string GeneratedImageUrl { get; set; }
        public bool HasGeneratedImageUrl => !string.IsNullOrWhiteSpace(GeneratedImageUrl);
    }
}
