using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Utilities
{
    public class RedditWebAgent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RedditConfiguration _redditConfiguration;
        private HttpClient httpClient;
        private string AccessToken;

        public RedditWebAgent(IHttpClientFactory httpClientFactory, IOptions<RedditConfiguration> redditConfigurations, ILogger<RedditWebAgent> logger)
        {
            _httpClientFactory = httpClientFactory;
            _redditConfiguration = redditConfigurations.Value;
        }

        public async Task Initialize()
        {
            httpClient = _httpClientFactory.CreateClient();            
        }

        private async Task Authenticate()
        {
            var formContent = GetAuthenticationFormUrlEncodedContent(!string.IsNullOrWhiteSpace(_redditConfiguration.RefreshToken));

            var httpRequestOptions = new HttpRequestOptions
            {
                HttpContent = formContent,
                HttpMethod = HttpMethod.Post,
                IsOauth = false,
                Uri = "access_token"
            };
        }

        public async Task<HttpResponseMessage> PostAsync(HttpRequestOptions httpRequestOptions, int retries)
        {
            var request = CreatePostRequest(httpRequestOptions);
            HttpResponseMessage response = null;

            for (int i = 0; i <= retries; i++)
            {
                var requestTask = request();
                response = await requestTask;

                //TODO: Rate limit method

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await Authenticate();
                    continue;
                }
                await Task.Delay(10000);
            }  
            return null;
        }

        private FormUrlEncodedContent GetAuthenticationFormUrlEncodedContent(bool isRefresh)
        {
            if (isRefresh)
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _redditConfiguration.RefreshToken)
                });
                return content;
            }
            else
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", _redditConfiguration.Code),
                    new KeyValuePair<string, string>("redirect_uri", _redditConfiguration.RedirectUri)
                });
                return content;
            }
        }

        private void SetBasicAuthenticationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{_redditConfiguration.ClientId}:{_redditConfiguration.ClientSecret}")));
        }

        private void SetBearerAuthenticationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", AccessToken);
        }

        private Func<Task<HttpResponseMessage>> CreateRequest(HttpRequestOptions httpRequestOptions)
        {
            if (httpRequestOptions.IsOauth)
            {
                SetBearerAuthenticationHeader();
            }
            else
            {
                SetBasicAuthenticationHeader();
            }

            var uriBuilder = new UriBuilder(httpRequestOptions.IsOauth ? _redditConfiguration.OauthUrl : _redditConfiguration.Url);
            httpClient.BaseAddress = uriBuilder.Uri;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                string.Format(_redditConfiguration.UserAgent, Assembly.GetExecutingAssembly().GetName().Version));
            
            switch (httpRequestOptions.HttpMethod.Method)
            {
                case "GET":
                    return new Func<Task<HttpResponseMessage>>(() =>
                    {
                        Console.WriteLine("Get");
                        return httpClient.GetAsync(httpRequestOptions.Uri);
                    });
                case "POST":
                    return new Func<Task<HttpResponseMessage>>(() =>
                    {
                        Console.WriteLine("Post");
                        return httpClient.PostAsync(httpRequestOptions.Uri, httpRequestOptions.HttpContent);
                    });
                default:
                    return new Func<Task<HttpResponseMessage>>(() =>
                    {
                        return null;
                    });
            }
        }

        public Func<Task<HttpResponseMessage>> CreatePostRequest(HttpRequestOptions httpRequestOptions)
        {
            httpRequestOptions.HttpMethod = HttpMethod.Post;
            var request = CreateRequest(httpRequestOptions);
            return request; 
        }

        public Func<Task<HttpResponseMessage>> CreateGetRequest(HttpRequestOptions httpRequestOptions)
        {
            httpRequestOptions.HttpMethod = HttpMethod.Get;
            var request = CreateRequest(httpRequestOptions);
            return request;
        }        
    }
}
