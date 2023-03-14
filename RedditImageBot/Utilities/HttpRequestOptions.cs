using System.Net.Http;

namespace RedditImageBot.Utilities
{
    public class InternalHttpRequestOptions
    {
        public HttpMethod HttpMethod { get; set; }
        public HttpContent HttpContent { get; set; }
        public bool IsOauth { get; set; }
        public int Retries { get; set; }
        public string Uri { get; set; }
        public int Timeout { get; set; } = 100;
    }
}
