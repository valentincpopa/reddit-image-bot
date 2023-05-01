using Microsoft.Extensions.Logging;
using RedditImageBot.Services.Abstractions;
using SixLabors.ImageSharp;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageProcessorFactory _imageProcessorFactory;
        private readonly ILogger<ImageService> _logger;

        public ImageService(
            IImageProcessorFactory imageProcessorFactory,
            ILogger<ImageService> logger)
        {
            _imageProcessorFactory = imageProcessorFactory;
            _logger = logger;
        }

        private static async Task<Stream> DownloadImageAsync(string imageUri)
        {
            var httpClient = new HttpClient();
            var imageStream = await httpClient.GetStreamAsync(imageUri);
            return imageStream;
        }

        public async Task<Stream> GenerateImageAsync(string text, string imageUri)
        {
            _logger.LogInformation("Started generating the image.");

            using var imageStream = await DownloadImageAsync(imageUri);
            using var imageToProcess = await Image.LoadAsync(imageStream);

            var decodedImageFormat = imageToProcess.Metadata.DecodedImageFormat;
            var imageProcessor = _imageProcessorFactory.CreateImageProcessor(decodedImageFormat.Name);
            var processedImageStream = await imageProcessor.ProcessAsync(imageToProcess, text);

            _logger.LogInformation("Finished generating the image.");
            return processedImageStream;
        }
    }
}
