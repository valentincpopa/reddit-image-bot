using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditImageBot.Processing.Pipelines;
using RedditImageBot.Services.WebAgents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class HostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<HostedService> _logger;
        private Timer _timer;

        public HostedService(IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<HostedService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called...");

            await ExecutePreProcessingActions();
            await Process();

            //_timer = new Timer(async (stateInfo) => await Process(), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }

        private async Task ExecutePreProcessingActions()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var redditWebAgent = scope.ServiceProvider.GetService<RedditWebAgent>();
            await redditWebAgent.Initialize();
        }

        public async Task Process()
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var redditWebAgent = scope.ServiceProvider.GetService<RedditWebAgent>();

                if (redditWebAgent.RateLimitData.RemainingRequests <= 0)
                {
                    _logger.LogWarning("The rate limit has been hit. Discarding the current pipeline workflow..");
                    return;
                }

                var pipelineWorker = new PipelineWorker(_serviceScopeFactory, _loggerFactory.CreateLogger<PipelineWorker>());
                await pipelineWorker.StartWorkflow();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Something went wrong during the pipeline processing.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called...");
            _timer.Change(Timeout.Infinite, 0);
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
