using RedditImageBot.Models;
using System;

namespace RedditImageBot.Database
{
    public class Message
    {
        public Message()
        {
            MessageStateManager = new MessageStateManager();
            ChangeState(MessageState.InProgress);
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        public int Id { get; }
        public string ExternalId { get; private set; }
        public int? PostId { get; set; }
        public Post Post { get; }
        public DateTime CreatedAt { get; }
        public DateTime ModifiedAt { get; private set; }
        public MessageState Status { get; private set; }
        public MessageStateManager MessageStateManager { get; }

        public void ChangeState(MessageState newState)
        {
            MessageStateManager.ChangeState(newState);
            Status = MessageStateManager.CurrentState;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
