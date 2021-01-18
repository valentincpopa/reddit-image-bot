using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class HostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HostedService> _logger;
        private readonly IBotService _botService;

        public HostedService(IConfiguration configuration, ILogger<HostedService> logger, IBotService botService)
        {
            _configuration = configuration;
            _logger = logger;
            _botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called with environment: {_configuration["DOTNET_ENV"]}");
            await _botService.InitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called");
            return Task.CompletedTask;
        }
    }
}
