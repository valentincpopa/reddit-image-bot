using RedditImageBot.Processing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RedditImageBot.Processing.Pipelines
{
    public class Pipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        private readonly List<IDataflowBlock> _dataflowBlocks;

        public Pipeline()
        {
            _dataflowBlocks = new List<IDataflowBlock>();
        }

        public Pipeline<TInput, TOutput> AddFilter<TInternalInput, TInternalOutput>
            (IPropagatorBlock<TInternalInput, TInternalOutput> targetBlock, DataflowLinkOptions dataflowLinkOptions)
        {
            if (!_dataflowBlocks.Any())
            {
                _dataflowBlocks.Add(targetBlock);
                return this;
            }

            var lastBlock = _dataflowBlocks.Last();
            if (lastBlock is ISourceBlock<TInternalInput> sourceBlock)
            {
                sourceBlock.LinkTo(targetBlock, dataflowLinkOptions);
                _dataflowBlocks.Add(targetBlock);
            }
            else
            {
                throw new Exception($"Cannot link the provided dataflow block because the last block in the pipeline is not a source block or the output doesn't match {typeof(TInternalInput)}.");
            }

            return this;
        }

        public Pipeline<TInput, TOutput> SendData(TInput input)
        {
            var firstBlock = _dataflowBlocks.First() as ITargetBlock<TInput>;
            firstBlock.Post(input);

            return this;
        }

        public async Task Complete()
        {
            var lastBlock = _dataflowBlocks.Last() as ISourceBlock<TOutput>;
            var actionBlock = new ActionBlock<TOutput>((x) => { });
            lastBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            var firstBlock = _dataflowBlocks.First();
            firstBlock.Complete();

            await actionBlock.Completion;
        }
    }
}
