using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditImageBot.Processing.Pipelines;
using RedditImageBot.Services.WebAgents;
using RedditImageBot.Utilities.Common;
using System;
using System.Runtime.CompilerServices;
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

        private static readonly string TypeFullName = typeof(HostedService).FullName;

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
            _ = Process();

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
            using var activity = ActivitySources.RedditImageBot.StartActivity(CreateActivityName());

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
                _logger.LogError(exception, "The pipeline processing stopped due to an unexpected error.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called...");
            _timer.Change(Timeout.Infinite, 0);
            _timer.Dispose();
            return Task.CompletedTask;
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{TypeFullName}.{callerMemberName}";
        }
    }
}
