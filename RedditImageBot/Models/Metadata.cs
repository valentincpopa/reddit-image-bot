using RedditImageBot.Utilities.Exceptions;
using System;

namespace RedditImageBot.Models
{
    public class Metadata
    {
        public Metadata(MessageMetadata messageMetadata)
        {
            MessageMetadata = messageMetadata ?? throw new ArgumentNullException(nameof(messageMetadata));
        }

        public MessageMetadata MessageMetadata { get; private set; }
        public PostMetadata PostMetadata { get; private set; }

        public Metadata SetupMetadata(MessageMetadata messageMetadata)
        {
            MessageMetadata = messageMetadata ?? throw new ArgumentNullException(nameof(messageMetadata));
            return this;
        }

        public Metadata SetupMetadata(PostMetadata postMetadata)
        {
            if (postMetadata == null)
            {
                throw new ArgumentNullException(nameof(postMetadata));
            }

            if (postMetadata.ExternalPostId != MessageMetadata.ExternalPostId)
            {
                throw new MetadataException($"The external id of the post referenced in the post metadata ({postMetadata.ExternalPostId})" +
                    $"should correspond to the external id of the post referenced in the message metadata ({MessageMetadata.ExternalPostId}).");
            }

            PostMetadata = postMetadata;
            return this;
        }
    }

}
