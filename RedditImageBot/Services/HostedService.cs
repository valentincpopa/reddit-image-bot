using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<HostedService> _logger;
        private Timer _timer;

        public HostedService(IServiceScopeFactory serviceScopeFactory, ILogger<HostedService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var botService = scope.ServiceProvider.GetRequiredService<IBotService>();
                await botService.InitializeAsync();
            }
            _timer = new Timer(async (stateInfo) => await PollAndResolveRequest(), null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
        }

        public async Task PollAndResolveRequest()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var botService = scope.ServiceProvider.GetRequiredService<IBotService>();
                    await botService.GenerateImagesAsync();
                }
                catch(Exception exception)
                {
                    _logger.LogError(exception.ToString());
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called");
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
