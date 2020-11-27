using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class HostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HostedService> _logger;
        private readonly RedditWebAgent _redditWebAgent;

        public HostedService(IConfiguration configuration, ILogger<HostedService> logger, RedditWebAgent redditWebAgent)
        {
            _configuration = configuration;
            _logger = logger;
            _redditWebAgent = redditWebAgent;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called with environment: {_configuration["DOTNET_ENV"]}");
            await _redditWebAgent.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called");
            return Task.CompletedTask;
        }
    }
}
