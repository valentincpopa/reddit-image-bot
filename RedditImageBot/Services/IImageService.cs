using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public interface IImageService
    {
        Task<MemoryStream> GenerateImageAsync(string title, string imageUri);
    }
}
