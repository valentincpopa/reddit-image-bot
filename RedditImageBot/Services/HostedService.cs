using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        public HostedService(IConfiguration configuration, ILogger<HostedService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called with environment: {_configuration["DOTNET_ENV"]}");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called");
            return Task.CompletedTask;
        }
    }
}
