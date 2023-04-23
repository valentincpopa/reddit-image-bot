using System.IO;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IImgurService
    {
        Task<string> UploadImageAsync(Stream memoryStream);
    }
}
