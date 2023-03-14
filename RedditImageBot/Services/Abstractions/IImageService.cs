using System.IO;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IImageService
    {
        Task<MemoryStream> GenerateImageAsync(string title, string imageUri);
    }
}
