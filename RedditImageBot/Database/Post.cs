using RedditImageBot.Models;
using System;
using System.Collections.Generic;

namespace RedditImageBot.Database
{
    public class Post
    {
        private PostState _status;

        public Post(string externalId, string generatedImageUrl)
        {
            ExternalId = externalId;
            GeneratedImageUrl = generatedImageUrl;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
            
            PostStateManager = new PostStateManager();
            Status = PostState.Processed;
        }

        public int Id { get; }
        public string ExternalId { get; }
        public string GeneratedImageUrl { get; private set; }
        public ICollection<Message> Messages { get; }
        public PostState Status 
        { 
            get
            {
                return _status;
            }
            private set 
            { 
                _status = value;
                PostStateManager.SetCurrentState(_status);
            } 
        }
        public PostStateManager PostStateManager { get; }
        public DateTime CreatedAt { get; }
        public DateTime ModifiedAt { get; private set; }

        public void ChangeState(PostState newState)
        {
            PostStateManager.ChangeState(newState);
            _status = PostStateManager.CurrentState;
            ModifiedAt = DateTime.UtcNow;
        }

        public void SetGeneratedImageUrl(string generatedImageUrl)
        {
            GeneratedImageUrl = generatedImageUrl;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
