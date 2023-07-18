using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities.Common;
using RedditImageBot.Utilities.Configurations;
using RedditImageBot.Utilities.Exceptions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    internal class MessageProcessorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<MessageProcessorFilter> _logger;
        private readonly IRedditService _redditService;
        private readonly BotInformationConfiguration _options;

        private static readonly string _typeFullName = typeof(MessageProcessorFilter).FullName;

        public MessageProcessorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            ILogger<MessageProcessorFilter> logger,
            IRedditService redditService,
            IOptions<BotInformationConfiguration> options)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
            _logger = logger;
            _redditService = redditService;
            _options = options.Value;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            using var activity = ActivitySources.RedditImageBot.StartActivity(CreateActivityName());

            _logger.LogInformation("Responding to the message with message id: {MessageId}..", metadata.MessageMetadata.ExternalCommentId);

            if (!metadata.MessageMetadata.InternalMessageId.HasValue)
            {
                throw new FilterProcessingException("The provided metadata doesn't contain the required information (messageId).");
            }

            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();

            var messageToUpdate = await applicationDbContext.Messages.FirstOrDefaultAsync(x => x.Id == metadata.MessageMetadata.InternalMessageId);
            messageToUpdate.ChangeState(MessageState.Processed);

            if (!metadata.PostMetadata.InternalPostId.HasValue)
            {
                throw new FilterProcessingException("The provided metadata doesn't contain the required information (postId).");
            }

            messageToUpdate.PostId = metadata.PostMetadata.InternalPostId;

            var responseMessage = metadata.PostMetadata.IsValidImage ?
                 $"The generated image can be found [here]({metadata.PostMetadata.GeneratedImageUrl}).\n\n" :
                 $"This post does not represent a valid image that I can process.\n\n";
            responseMessage = responseMessage +
                $"*^(I)* *^(am)* *^(a)* *^(bot)* *^(and)* *^(this)* *^(action)* *^(was)* *^(performed)* *^(automatically.)* " +
                $"*^(Please)* [*^(contact the creator of this bot)*](/message/compose/?to=/u/{_options.CreatorUsername}) *^(if)* *^(you)* *^(have)* *^(any)* *^(questions)* *^(or)* *^(concerns.)* " +
                $"*^(You)* *^(can)* *^(find)* *^(the)* *^(source)* *^(code)* [*^(here)*]({_options.SourceCodeUrl})*^(.)*";


            // TODO:
            //var repliesAuthors = await _redditService.GetCommentRepliesAuthorsAsync(metadata.PostMetadata.ExternalPostId, metadata.MessageMetadata.ExternalCommentId);

            //if (!repliesAuthors.Any(x => x == _options.BotUsername))
            //{
            //    await _redditService.ReplyAsync(metadata.MessageMetadata.ExternalCommentId, responseMessage);
            //}

            await _redditService.ReplyAsync(metadata.MessageMetadata.ExternalCommentId, responseMessage);

            await applicationDbContext.SaveChangesAsync();

            return metadata;
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{_typeFullName}.{callerMemberName}";
        }
    }
}
