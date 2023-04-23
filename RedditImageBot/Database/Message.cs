using RedditImageBot.Models;
using System;

namespace RedditImageBot.Database
{
    public class Message
    {
        private MessageState _status;

        public Message()
        {
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
            MessageStateManager = new MessageStateManager();
        }

        public int Id { get; }
        public string ExternalId { get; private set; }
        public string ExternalParentId { get; private set; }
        public int? PostId { get; set; }
        public Post Post { get; }
        public DateTime CreatedAt { get; }
        public DateTime ModifiedAt { get; private set; }
        public MessageState Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                MessageStateManager.SetCurrentState(_status);
            }
        }
        public MessageStateManager MessageStateManager { get; }
        public uint Version { get; }

        public void ChangeState(MessageState newState)
        {
            MessageStateManager.ChangeState(newState);
            _status = MessageStateManager.CurrentState;
            ModifiedAt = DateTime.UtcNow;
        }

        public void ResetState()
        {
            Status = MessageState.NotProcessed;
        }
    }
}
