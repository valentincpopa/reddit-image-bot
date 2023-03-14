using System;

namespace RedditImageBot.Models
{
    public class Metadata
    {
        public Metadata(MessageMetadata messageMetadata)
        {
            MessageMetadata = messageMetadata;
        }

        public MessageMetadata MessageMetadata { get; private set; }
        public PostMetadata PostMetadata { get; private set; }

        public Metadata SetupMetadata(MessageMetadata messageMetadata)
        {
            MessageMetadata = messageMetadata;
            return this;
        }

        public Metadata SetupMetadata(PostMetadata postMetadata)
        {
            if (postMetadata.ExternalId != MessageMetadata.ExternalPostId)
            {
                throw new InvalidOperationException("Invalid operation.");
            }

            PostMetadata = postMetadata;
            return this;
        }
    }

}
