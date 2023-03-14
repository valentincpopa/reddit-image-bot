namespace RedditImageBot.Models
{
    public enum MessageState { NotProcessed, InProgress, Processed, Error }

    public class MessageStateManager : StateManager<MessageState>
    {
        public MessageStateManager() : base()
        {
            SetInitialState(MessageState.NotProcessed);
            AddTransition(MessageState.NotProcessed, MessageState.InProgress);
            AddTransition(MessageState.InProgress, MessageState.Processed);
            AddTransition(MessageState.InProgress, MessageState.Error);
        }
    }
}
