namespace RedditImageBot.Models
{
    public enum PostState { NotProcessed, InProgress, Processed, Error }

    public class PostStateManager : StateManager<PostState>
    {
        public PostStateManager() : base()
        {
            SetInitialState(PostState.NotProcessed);
            AddTransition(PostState.NotProcessed, PostState.InProgress);
            AddTransition(PostState.InProgress, PostState.Processed);
            AddTransition(PostState.InProgress, PostState.Error);
        }
    }
}
