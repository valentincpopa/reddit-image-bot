using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using System.Threading.Tasks;

namespace RedditImageBot.Processing.Filters
{
    public class PostProcessorFilter
    {
        private readonly IDbContextFactory<ApplicationDbContext> _applicationDbContextFactory;
        private readonly ILogger<PostProcessorFilter> _logger;
        private readonly IImageService _imageService;
        private readonly IImgurService _imgurService;

        public PostProcessorFilter(
            IDbContextFactory<ApplicationDbContext> applicationDbContextFactory,
            ILogger<PostProcessorFilter> logger,
            IImageService imageService,
            IImgurService imgurService)
        {
            _imageService = imageService;
            _imgurService = imgurService;
            _applicationDbContextFactory = applicationDbContextFactory;
            _logger = logger;
        }

        public async Task<Metadata> Process(Metadata metadata)
        {
            if (!metadata.PostMetadata.HasGeneratedImageUrl)
            {
                if (metadata.PostMetadata.IsValidImage)
                {
                    _logger.LogInformation("Generating the image for {ExternalPostId}..", metadata.PostMetadata.ExternalId);
                    using var generatedImage = await _imageService.GenerateImageAsync(metadata.PostMetadata.PostTitle, metadata.PostMetadata.OriginalImageUrl);
                    metadata.PostMetadata.SetupGeneratedImageUrl(await _imgurService.UploadImageAsync(generatedImage));
                }

                using var applicationDbContext = await _applicationDbContextFactory.CreateDbContextAsync();

                var processedPost = new Post(metadata.PostMetadata.ExternalId, metadata.PostMetadata.GeneratedImageUrl);
                await applicationDbContext.Posts.AddAsync(processedPost);
                await applicationDbContext.SaveChangesAsync();

                metadata.PostMetadata.PostId = processedPost.Id;
            }

            return metadata;
        }

    }
}
