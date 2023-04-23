using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Utilities.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class BotMessageRescheduler : IHostedService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<BotProcessingService> _logger;
        private Timer _timer;

        private static readonly string _typeFullName = typeof(BotMessageRescheduler).FullName;

        public BotMessageRescheduler(ILogger<BotProcessingService> logger, IDbContextFactory<ApplicationDbContext> applicationDbContextFactory)
        {
            _logger = logger;
            _applicationDbContextFactory = applicationDbContextFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"StartAsync has been called...");

            _timer = new Timer(async (stateInfo) => await Process(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async Task Process()
        {
            using var activity = ActivitySources.RedditImageBot.StartActivity(CreateActivityName());

            var applicationDbContext = _applicationDbContextFactory.CreateDbContext();

            var datetimeToCompare = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));

            var messagesWithErrors = applicationDbContext.Messages
                .Where(x =>
                    (x.Status == MessageState.Error || x.Status == MessageState.InProgress)
                    && x.ModifiedAt < datetimeToCompare);

            foreach (var message in messagesWithErrors)
            {
                message.ResetState();
            }

            await applicationDbContext.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called...");
            return Task.CompletedTask;
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{_typeFullName}.{callerMemberName}";
        }
    }
}
