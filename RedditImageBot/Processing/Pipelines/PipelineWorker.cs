﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Processing.Filters;
using RedditImageBot.Services.Abstractions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RedditImageBot.Processing.Pipelines
{
    public class PipelineWorker
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<PipelineWorker> _logger;

        public PipelineWorker(IServiceScopeFactory serviceScopeFactory, ILogger<PipelineWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task StartWorkflow()
        {
            _logger.LogInformation("Started the pipeline processing.");

            using var scope = _serviceScopeFactory.CreateScope();

            var pipeline = new Pipeline<string, Task>();

            var messageReaderFilter = new MessageReaderFilter(
                scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>(),
                scope.ServiceProvider.GetService<ILogger<MessageReaderFilter>>(),
                scope.ServiceProvider.GetService<IRedditService>(),
                scope.ServiceProvider.GetService<IMapper>());

            var postValidatorFilter = new PostValidatorFilter(
                scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>(),
                scope.ServiceProvider.GetService<IRedditService>(),
                scope.ServiceProvider.GetService<ILogger<PostValidatorFilter>>());

            var postProcessorFilter = new PostProcessorFilter(
                scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>(),
                scope.ServiceProvider.GetService<ILogger<PostProcessorFilter>>(),
                scope.ServiceProvider.GetService<IImageService>(),
                scope.ServiceProvider.GetService<IImgurService>());

            var messageProcessorFilter = new MessageProcessorFilter(
                scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>(),
                scope.ServiceProvider.GetService<ILogger<MessageProcessorFilter>>(),
                scope.ServiceProvider.GetService<IRedditService>());

            pipeline
                .AddFilter(
                    new TransformManyBlock<string, Metadata>(
                        messageReaderFilter.Process,
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }),
                    new DataflowLinkOptions { PropagateCompletion = true })
                .AddFilter(
                    new TransformBlock<Metadata, Metadata>(
                        postValidatorFilter.Process,
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }),
                    new DataflowLinkOptions { PropagateCompletion = true })
                .AddFilter(
                    new TransformBlock<Metadata, Metadata>(
                        postProcessorFilter.Process,
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }),
                    new DataflowLinkOptions { PropagateCompletion = true })
                .AddFilter(
                    new TransformBlock<Metadata, Metadata>(
                        messageProcessorFilter.Process,
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }),
                    new DataflowLinkOptions { PropagateCompletion = true });

            pipeline.SendData(string.Empty);

            await pipeline.Complete();

            _logger.LogInformation("Completed the pipeline processing.");
        }
    }
}
