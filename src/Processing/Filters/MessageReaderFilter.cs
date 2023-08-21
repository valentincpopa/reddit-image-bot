using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    public class MessageReaderFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<MessageReaderFilter> _logger;
        private readonly IRedditService _redditService;
        private readonly IMapper _mapper;

        private static readonly string _typeFullName = typeof(MessageReaderFilter).FullName;

        public MessageReaderFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            ILogger<MessageReaderFilter> logger,
            IRedditService redditService,
            IMapper mapper)
        {
            _redditService = redditService;
            _applicationDbContextFactory = applicationDbContextFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Metadata>> Process(string _)
        {
            using var activity = ActivitySources.RedditImageBot.StartActivity(CreateActivityName());

            _logger.LogInformation("Started processing unread messages..");

            var inboxMessages = await _redditService.GetUnreadMessagesAsync();
            var messagesExternalIds = inboxMessages.Select(x => x.Name);

            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();
            var databaseMessagesIds = await applicationDbContext.Messages
                .Where(x => messagesExternalIds.Contains(x.ExternalId))
                .Select(x => x.ExternalId)
                .ToListAsync();

            var messagesToInsert = _mapper.Map<List<Message>>(inboxMessages.Where(x => !databaseMessagesIds.Contains(x.Name)));
            await applicationDbContext.Messages.AddRangeAsync(messagesToInsert);
            await applicationDbContext.SaveChangesAsync();

            if (inboxMessages.Any())
            {
                await _redditService.ReadMessagesAsync(string.Join(',', inboxMessages.Select(x => x.Name)));
            }

            var unprocessedMessages = await applicationDbContext.Messages
                .Where(x => x.Status == MessageState.NotProcessed)
                .OrderBy(x => x.CreatedAt)
                .Take(25)
                .ToListAsync();

            foreach (var message in unprocessedMessages)
            {
                message.ChangeState(MessageState.InProgress);
            }

            MeterSources.MessageCounter.Add(unprocessedMessages.Count);

            await applicationDbContext.SaveChangesAsync();

            var metadataCollection = unprocessedMessages.Select(x => new Metadata(_mapper.Map<MessageMetadata>(x)));

            return metadataCollection;
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{_typeFullName}.{callerMemberName}";
        }
    }
}
