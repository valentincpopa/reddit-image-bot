using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    public class PostValidatorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly IRedditService _redditService;
        private readonly ILogger<PostValidatorFilter> _logger;

        public PostValidatorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            IRedditService redditService,
            ILogger<PostValidatorFilter> logger)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
            _redditService = redditService;
            _logger = logger;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            _logger.LogInformation("Started processing the message identified by the following external id: {ExternalMessageId}", metadata.MessageMetadata.ExternalMessageId);

            var post = await _redditService.GetPostAsync(metadata.MessageMetadata.ExternalPostId);
            if (!post.IsRedditImage)
            {
                _logger.LogWarning("The requested post ({ExternalPostId}) does not represent an image or its source does not represent a reddit media domain.", metadata.MessageMetadata.ExternalPostId);
                metadata.SetupMetadata(new PostMetadata(metadata.MessageMetadata.ExternalPostId, false));
                return metadata;
            }

            var postMetadata = new PostMetadata(metadata.MessageMetadata.ExternalPostId, post.Title, post.Url);

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
    }
}
