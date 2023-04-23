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
    public class GifImageProcessor : BaseImageProcessor, IImageProcessor
    {
        public GifImageProcessor(IOptions<ImageConfiguration> options) : base(options)
        {
        }

        public async Task<Stream> ProcessAsync(Image imageToProcess, string text)
        {
            (Font font, FontRectangle fontRectangle) = GetOptimalFontProperties(imageToProcess.Height, imageToProcess.Width, text);

            var textOptions = GetTextOptions(font, imageToProcess.Width);
            using var outputImage = new Image<Rgba32>(imageToProcess.Width, (int)fontRectangle.Height + imageToProcess.Height);

            var imageToProcessMetadata = imageToProcess.Metadata.GetGifMetadata();
            var outputImageMetadata = outputImage.Metadata.GetGifMetadata();
            outputImageMetadata.RepeatCount = imageToProcessMetadata.RepeatCount;

            var imageToProcessRootFrameMetadata = imageToProcess.Frames.RootFrame.Metadata.GetGifMetadata();
            var outputImageRootFrameMetadata = outputImage.Frames.RootFrame.Metadata.GetGifMetadata();
            outputImageRootFrameMetadata.FrameDelay = imageToProcessRootFrameMetadata.FrameDelay;

            for (int i = 0; i < imageToProcess.Frames.Count; i++)
            {
                using var image = new Image<Rgba32>(imageToProcess.Width, (int)fontRectangle.Height + imageToProcess.Height);
                image.Mutate(x => x.BackgroundColor(Color.White, new Rectangle((int)fontRectangle.X, (int)fontRectangle.Y, (int)imageToProcess.Width, (int)fontRectangle.Height)));
                image.Mutate(x => x.DrawText(textOptions, text, Color.Black));
                image.Mutate(x => x.DrawImage(imageToProcess.Frames.CloneFrame(i), new Point(0, (int)fontRectangle.Height), 1f));

                var metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = imageToProcessRootFrameMetadata.FrameDelay;

                outputImage.Frames.AddFrame(image.Frames.RootFrame);
            }

            var outputStream = new MemoryStream();
            outputImage.Frames.RemoveFrame(0);

            await outputImage.SaveAsync(outputStream, imageToProcess.Metadata.DecodedImageFormat);

            return outputStream;
        }


    }
}
