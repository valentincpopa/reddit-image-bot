using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class ImgurService : IImgurService
    {
        private readonly ImgurWebAgentConfiguration _options;
        private readonly ILogger<ImgurService> _logger;

        public ImgurService(IOptions<ImgurWebAgentConfiguration> options, ILogger<ImgurService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(MemoryStream memoryStream)
        {
            var apiClient = new ApiClient(_options.ClientId);
            var httpClient = new HttpClient();

            var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
            var retryPolicy = GetRetryPolicy();
            var imageLink = await retryPolicy.ExecuteAsync(() => UploadImageAsync(imageEndpoint, memoryStream));

            await memoryStream.DisposeAsync();
            return imageLink;
        }

        private static async Task<string> UploadImageAsync(ImageEndpoint imageEndpoint, MemoryStream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var imgurImage = await imageEndpoint.UploadImageAsync(memoryStream);
            return imgurImage.Link;
        }

        private AsyncRetryPolicy GetRetryPolicy()
        {
            return Policy
              .Handle<Exception>()
              .WaitAndRetryAsync(new[]
              {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
              }, onRetry: (response, delay, retryCount, context) =>
              {
                  _logger.LogWarning(response, "Something went wrong during the upload of the image to the remote host. [{retryCount}] Retrying..", retryCount);
              });
        }
    }
}
