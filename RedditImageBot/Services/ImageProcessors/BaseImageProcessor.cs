using Microsoft.Extensions.Options;
using RedditImageBot.Utilities;
using SixLabors.Fonts;
using System.IO;

namespace RedditImageBot.Services.ImageProcessors
{
    public abstract class BaseImageProcessor
    {
        private readonly ImageConfiguration _options;

        public BaseImageProcessor(IOptions<ImageConfiguration> options)
        {
            _options = options.Value;
        }

        protected (Font font, FontRectangle fontRectangle) GetOptimalFontProperties(int height, int width, string text)
        {
            int fontSize = 1;
            var fontFamily = GetFontFamily();
            var font = fontFamily.CreateFont(fontSize);

            var fontRectangle = TextMeasurer.Measure(text, new TextOptions(font) { WrappingLength = width });

            int reference;
            if (height / width > 3)
            {
                reference = width;
            }
            else
            {
                reference = height;
            }

            while (fontRectangle.Height < reference * _options.Scale)
            {
                fontSize++;
                font = fontFamily.CreateFont(fontSize);
                fontRectangle = TextMeasurer.Measure(text, new TextOptions(font) { WrappingLength = width });
            }

            return (font, fontRectangle);
        }

        protected static TextOptions GetTextOptions(Font font, float imageWidth)
        {

            return new TextOptions(font)
            {
                KerningMode = KerningMode.Standard,
                HorizontalAlignment = HorizontalAlignment.Left,
                WrappingLength = imageWidth
            };

        }

        protected FontFamily GetFontFamily()
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(Path.Combine(Path.GetDirectoryName(typeof(BaseImageProcessor).Assembly.Location), _options.TitleFont));
            return fontFamily;
        }
    }
}
