using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Utilities
{
    public class RedditWebAgent
    {
        private readonly HttpClient _httpClient;

        public RedditConfiguration RedditConfiguration { get; set; }

        public RedditWebAgent(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task Initialize()
        {
            
        }

        private FormUrlEncodedContent GetAuthenticationFormUrlEncodedContent(bool isRefresh)
        {
            if (isRefresh)
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("code", RedditConfiguration.Code),
                    new KeyValuePair<string, string>("refresh_token", RedditConfiguration.RefreshToken)
                });
                return content;
            }
            else
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", RedditConfiguration.Code),
                    new KeyValuePair<string, string>("redirect_uri", RedditConfiguration.RedirectUri)
                });
                return content;
            }
        }
    }
}
