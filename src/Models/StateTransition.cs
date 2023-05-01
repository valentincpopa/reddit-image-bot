namespace RedditImageBot.Models
{
    public class StateTransition<T>
    {
        public StateTransition(T source, T destionation)
        {
            Source = source;
            Destination = destionation;
        }

        public T Source { get; }
        public T Destination { get; }
    }
}
