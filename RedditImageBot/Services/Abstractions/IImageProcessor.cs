using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IImageProcessor
    {
        Task<Stream> ProcessAsync(Image imageToProcess, string text);
    }
}
