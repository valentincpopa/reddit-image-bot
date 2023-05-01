using RedditImageBot.Utilities;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IWebAgent
    {
        Task Initialize();
        Task<HttpResponseMessage> SendRequestAsync(InternalHttpRequestOptions httpRequestOptions);
    }
}