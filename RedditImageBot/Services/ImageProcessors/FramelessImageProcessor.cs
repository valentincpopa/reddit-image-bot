using Microsoft.Extensions.Options;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace RedditImageBot.Services.ImageProcessors
{
    public class FramelessImageProcessor : BaseImageProcessor, IImageProcessor
    {
        public FramelessImageProcessor(IOptions<ImageConfiguration> options) : base(options)
        {
        }

        public async Task<Stream> ProcessAsync(Image imageToProcess, string text)
        {
            (Font font, FontRectangle fontRectangle) = GetOptimalFontProperties(imageToProcess.Height, imageToProcess.Width, text);

            var textOptions = GetTextOptions(font, imageToProcess.Width);
            using var outputImage = new Image<Rgba32>(imageToProcess.Width, (int)fontRectangle.Height + imageToProcess.Height, Color.White);

            outputImage.Mutate(x => x.DrawText(textOptions, text, Color.Black));
            outputImage.Mutate(x => x.DrawImage(imageToProcess, new Point(0, (int)fontRectangle.Height), 1f));

            var outputStream = new MemoryStream();
            await outputImage.SaveAsync(outputStream, imageToProcess.Metadata.DecodedImageFormat);

            return outputStream;
        }
    }
}
