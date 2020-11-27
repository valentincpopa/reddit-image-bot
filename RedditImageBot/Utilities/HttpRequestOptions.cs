using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace RedditImageBot.Utilities
{
    public class HttpRequestOptions
    {
        public HttpMethod HttpMethod { get; set; }
        public HttpContent HttpContent { get; set; }
        public bool IsOauth { get; set; }
        public int Retries { get; set; }
        public string Uri { get; set; }
        public int Timeout { get; set; } = 100;
    }
}
