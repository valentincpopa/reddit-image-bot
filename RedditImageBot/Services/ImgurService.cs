using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities;
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
            _logger.LogInformation("Uploading the generated image to Imgur...");

            var apiClient = new ApiClient(_options.ClientId);
            var httpClient = new HttpClient();

            var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var imgurImage = await imageEndpoint.UploadImageAsync(memoryStream);
            await memoryStream.DisposeAsync();
            return imgurImage.Link;
        }
    }
}
