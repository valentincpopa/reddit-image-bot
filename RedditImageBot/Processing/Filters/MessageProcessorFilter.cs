using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities.Configurations;
using RedditImageBot.Utilities.Exceptions;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    internal class MessageProcessorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<MessageProcessorFilter> _logger;
        private readonly IRedditService _redditService;
        private readonly RedditAccountConfiguration _options;

        public MessageProcessorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            ILogger<MessageProcessorFilter> logger,
            IRedditService redditService,
            IOptions<RedditAccountConfiguration> options)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
            _logger = logger;
            _redditService = redditService;
            _options = options.Value;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            _logger.LogInformation("Responding to the message with message id: {MessageId}..", metadata.MessageMetadata.ExternalCommentId);

            if (!metadata.MessageMetadata.MessageId.HasValue)
            {
                throw new FilterProcessingException("The provided metadata doesn't contain the required information (messageId).");
            }

            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();

            var messageToUpdate = await applicationDbContext.Messages.FirstOrDefaultAsync(x => x.Id == metadata.MessageMetadata.MessageId);
            messageToUpdate.ChangeState(MessageState.Processed);

            if (!metadata.PostMetadata.PostId.HasValue)
            {
                throw new FilterProcessingException("The provided metadata doesn't contain the required information (postId).");
            }

            messageToUpdate.PostId = metadata.PostMetadata.PostId;
            var postToUpdate = await applicationDbContext.Posts.FirstOrDefaultAsync(x => x.Id == metadata.PostMetadata.PostId);
            postToUpdate.ChangeState(PostState.Processed);

            var responseMessage = metadata.PostMetadata.IsValidImage ?
                 $"The generated image can be found here: {metadata.PostMetadata.GeneratedImageUrl}.\n\n" :
                 $"This post does not represent a valid image that I can process.\n\n";
            responseMessage = responseMessage +
                $"*I am a bot, and this action was performed automatically. " +
                $"Please [contact the creator of this bot](/message/compose/?to=/u/{_options.Username}) if you have any questions or concerns.*";

            await _redditService.ReplyAsync(metadata.MessageMetadata.ExternalCommentId, responseMessage);
            await _redditService.ReadMessageAsync(metadata.MessageMetadata.ExternalId);

            await applicationDbContext.SaveChangesAsync();

            return metadata;
        }
    }
}
