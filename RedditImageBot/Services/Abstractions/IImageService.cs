using System.IO;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IImageService
    {
        Task<Stream> GenerateImageAsync(string text, string imageUri);
    }
}
