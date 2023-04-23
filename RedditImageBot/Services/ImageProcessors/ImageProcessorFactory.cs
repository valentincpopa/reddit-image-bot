using Microsoft.Extensions.DependencyInjection;
using RedditImageBot.Services.Abstractions;
using System;

namespace RedditImageBot.Services.ImageProcessors
{
    public class ImageProcessorFactory : IImageProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ImageProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IImageProcessor CreateImageProcessor(string imageFormat)
        {
            switch (imageFormat)
            {
                case "GIF":
                    return _serviceProvider.GetService<GifImageProcessor>();
                default:
                    return _serviceProvider.GetService<FramelessImageProcessor>();
            }
        }
    }
}
