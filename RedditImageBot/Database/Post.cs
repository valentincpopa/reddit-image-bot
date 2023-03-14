using RedditImageBot.Models;
using System;
using System.Collections.Generic;

namespace RedditImageBot.Database
{
    public class Post
    {
        public Post(string externalId, string generatedImageUrl)
        {
            PostStateManager = new PostStateManager();
            ChangeState(PostState.InProgress);
            ExternalId = externalId;
            GeneratedImageUrl = generatedImageUrl;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        public int Id { get; }
        public string ExternalId { get; }
        public string GeneratedImageUrl { get; }
        public ICollection<Message> Messages { get; }
        public PostState Status { get; private set; }
        public PostStateManager PostStateManager { get; }
        public DateTime CreatedAt { get; }
        public DateTime ModifiedAt { get; private set; }

        public void ChangeState(PostState newState)
        {
            PostStateManager.ChangeState(newState);
            Status = PostStateManager.CurrentState;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
