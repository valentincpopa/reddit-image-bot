using RedditImageBot.Processing.Pipelines;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RedditImageBot.Processing.Abstractions
{
    public interface IPipeline<TInput, TOutput>
    {
        Pipeline<TInput, TOutput> AddFilter<TInternalInput, TInternalOutput>(
            IPropagatorBlock<TInternalInput, TInternalOutput> targetBlock, DataflowLinkOptions dataflowLinkOptions);
        Pipeline<TInput, TOutput> SendData(TInput input);
        Task Complete();
    }
}