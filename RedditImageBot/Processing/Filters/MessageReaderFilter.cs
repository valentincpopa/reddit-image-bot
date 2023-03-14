using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    public class MessageReaderFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<MessageReaderFilter> _logger;
        private readonly IRedditService _redditService;
        private readonly IMapper _mapper;

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
            _logger.LogInformation("Started processing unread messages..");

            var inboxMessages = await _redditService.GetUnreadMessagesAsync();
            var messagesExternalIds = inboxMessages.Select(x => x.Name);
            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();
            var databaseMessagesIds = await applicationDbContext.Messages
                .Where(x => messagesExternalIds.Contains(x.ExternalId))
                .Select(x => x.ExternalId)
                .ToListAsync();
            var unprocessedMessages = _mapper.Map<List<Message>>(inboxMessages.Where(x => !databaseMessagesIds.Contains(x.Name)));

            await applicationDbContext.Messages.AddRangeAsync(unprocessedMessages);
            await applicationDbContext.SaveChangesAsync();

            var metadataCollection = new List<Metadata>();
            foreach (var unprocessedMessage in unprocessedMessages)
            {
                var metadata = new Metadata(_mapper.Map<MessageMetadata>(unprocessedMessage));
                metadata.MessageMetadata.ExternalPostId = inboxMessages.First(x => x.Name == metadata.MessageMetadata.ExternalId).ParentId;
                metadataCollection.Add(metadata);
            }
            
            return metadataCollection;
        }
    }
}
