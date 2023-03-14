using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using System;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    internal class MessageProcessorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<MessageProcessorFilter> _logger;
        private readonly IRedditService _redditService;


        public MessageProcessorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            ILogger<MessageProcessorFilter> logger,
            IRedditService redditService)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
            _logger = logger;
            _redditService = redditService;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            _logger.LogInformation("Responding to the message with message id: {MessageId}..", metadata.MessageMetadata.ExternalCommentId);

            if (!metadata.MessageMetadata.MessageId.HasValue)
            {
                throw new InvalidOperationException("Invalid operation.");
            }

            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();

            var messageToUpdate = await applicationDbContext.Messages.FirstOrDefaultAsync(x => x.Id == metadata.MessageMetadata.MessageId);
            messageToUpdate.ChangeState(MessageState.Processed);

            if (!metadata.PostMetadata.PostId.HasValue)
            {
                throw new InvalidOperationException("Invalid operation.");
            }

            messageToUpdate.PostId = metadata.PostMetadata.PostId;
            var postToUpdate = await applicationDbContext.Posts.FirstOrDefaultAsync(x => x.Id == metadata.PostMetadata.PostId);
            postToUpdate.ChangeState(PostState.Processed);

            var responseMessage = metadata.PostMetadata.IsValidImage ?
                 $"The generated image can be found here: {metadata.PostMetadata.GeneratedImageUrl}.\n\n" :
                 $"This post does not represent a valid image that I can process.\n\n";
            responseMessage = responseMessage +
                $"*I am a bot, and this action was performed automatically. " +
                $"Please [contact the creator of this bot](/message/compose/?to=/u/replace-this-random-name) if you have any questions or concerns.*";

            await _redditService.ReplyAsync(metadata.MessageMetadata.ExternalCommentId, responseMessage);
            
            await applicationDbContext.SaveChangesAsync();

            return metadata;
        }
    }
}
