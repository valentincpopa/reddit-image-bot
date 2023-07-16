using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    public class PostValidatorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly IRedditService _redditService;
        private readonly ILogger<PostValidatorFilter> _logger;
        private readonly IMessageParserService _messageParserService;

        private static readonly string _typeFullName = typeof(PostValidatorFilter).FullName;

        public PostValidatorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            IRedditService redditService,
            IMessageParserService messageParserService,
            ILogger<PostValidatorFilter> logger)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
            _messageParserService = messageParserService;
            _redditService = redditService;
            _logger = logger;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            using var activity = ActivitySources.RedditImageBot.StartActivity(CreateActivityName());

            _logger.LogInformation("Started processing the message identified by the following external id: {ExternalMessageId}", metadata.MessageMetadata.ExternalMessageId);

            var parsedBody = _messageParserService.Parse(metadata.MessageMetadata.Body);
            if (parsedBody.IsValid)
            {
                metadata.MessageMetadata.ExternalPostId = parsedBody.ExternalPostId;
            }

            var post = await _redditService.GetPostAsync(metadata.MessageMetadata.ExternalPostId);
            if (!post?.IsImage ?? true)
            {
                _logger.LogWarning("The requested post ({ExternalPostId}) does not represent an image or its source does not represent a reddit media domain.", metadata.MessageMetadata.ExternalPostId);
                metadata.SetupMetadata(new PostMetadata(metadata.MessageMetadata.ExternalPostId, false));
                return metadata;
            }

            var postMetadata = new PostMetadata(metadata.MessageMetadata.ExternalPostId, parsedBody?.Title ?? post.Title, post.Url);

            using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();

            var processedPost = await applicationDbContext.Posts.FirstOrDefaultAsync(x => x.ExternalId == postMetadata.ExternalPostId);
            if (processedPost != null)
            {
                postMetadata.GeneratedImageUrl = processedPost.GeneratedImageUrl;
                postMetadata.InternalPostId = processedPost.Id;
            }

            metadata.SetupMetadata(postMetadata);
            return metadata;
        }

        private static string CreateActivityName([CallerMemberName] string callerMemberName = "")
        {
            return $"{_typeFullName}.{callerMemberName}";
        }
    }
}
