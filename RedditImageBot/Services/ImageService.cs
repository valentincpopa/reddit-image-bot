using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class ImageService : IImageService
    {
        private readonly ImageConfiguration _options;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IOptions<ImageConfiguration> options, ILogger<ImageService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        private async Task<Stream> DownloadImageAsync(string imageUri)
        {
            var httpClient = new HttpClient();
            var imageStream = await httpClient.GetStreamAsync(imageUri);
            return imageStream;
        }

        public async Task<MemoryStream> GenerateImageAsync(string title, string imageUri)
        {
            var imageStream = await DownloadImageAsync(imageUri);
            var redditImage = await Image.LoadAsync(imageStream);

            var options = GetTextGraphicsOptions(redditImage.Width);

            int fontSize = 1;
            var fontFamily = GetFontFamily();
            var font = fontFamily.CreateFont(fontSize);

            var textImageSize = TextMeasurer.Measure(title, new RendererOptions(font) { WrappingWidth = redditImage.Width });

            var reference = 0;
            if (redditImage.Height / redditImage.Width > 3)
            {
                reference = redditImage.Width;
            }
            else
            {
                reference = redditImage.Height;
            }

            while (textImageSize.Height < reference * _options.Scale)
            {
                fontSize++;
                font = fontFamily.CreateFont(fontSize);
                textImageSize = TextMeasurer.Measure(title, new RendererOptions(font) { WrappingWidth = redditImage.Width });
            }

            var outputImage = new Image<Rgba32>(redditImage.Width, (int)textImageSize.Height + redditImage.Height, Color.White);
            outputImage.Mutate(x => x.DrawText(options, title, font, Color.Black, new PointF(0, 0)));
            outputImage.Mutate(x => x.DrawImage(redditImage, new Point(0, (int)textImageSize.Height), 1f));

            var outputStream = new MemoryStream();
            outputStream.Seek(0, SeekOrigin.Begin);
            await outputImage.SaveAsync(outputStream, new JpegEncoder());

            return outputStream;
        }

        private TextGraphicsOptions GetTextGraphicsOptions(float imageWidth)
        {
            return new TextGraphicsOptions()
            {
                TextOptions = new TextOptions
                {
                    ApplyKerning = true,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WrapTextWidth = imageWidth
                }
            };
        }

        private FontFamily GetFontFamily()
        {
            FontCollection collection = new FontCollection();
            FontFamily family = collection.Install(Path.Combine(Path.GetDirectoryName(typeof(ImageService).Assembly.Location), _options.TitleFont));
            return family;
        }
    }
}
