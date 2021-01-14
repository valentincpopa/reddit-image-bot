using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditImageBot.Models;
using RedditImageBot.Services;
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
        private string AccessToken;

        public RedditWebAgent(IHttpClientFactory httpClientFactory, IOptions<RedditConfiguration> redditConfigurations, ILogger<RedditWebAgent> logger)
        {
            _httpClientFactory = httpClientFactory;
            _redditConfiguration = redditConfigurations.Value;
        }

        public async Task Initialize()
        {            
            await Authenticate();
        }

        private async Task Authenticate()
        {            
            var refreshTokenExists = !string.IsNullOrWhiteSpace(_redditConfiguration.RefreshToken);
            var formContent = GetAuthenticationFormUrlEncodedContent(refreshTokenExists);

            var httpRequestOptions = new HttpRequestOptions
            {
                HttpContent = formContent,
                HttpMethod = HttpMethod.Post,
                IsOauth = false,
                Uri = "access_token"
            };

            var request = CreateRequest(httpRequestOptions);
            var requestTask = request();
            var response = await requestTask;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
                if (!refreshTokenExists)
                {
                    _redditConfiguration.RefreshToken = authenticationResponse.RefreshToken;
                }
                //TODO: Synchronization/Authentication information extraction into another class
                AccessToken = authenticationResponse.AccessToken;               
            }
            
        }

        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestOptions httpRequestOptions)
        {
            var request = CreateRequest(httpRequestOptions);
            HttpResponseMessage response = null;

            for (int i = 0; i <= httpRequestOptions.Retries; i++)
            {
                var requestTask = request();
                response = await requestTask;

                //TODO: Rate limit method

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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

        private void SetBasicAuthenticationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{_redditConfiguration.ClientId}:{_redditConfiguration.ClientSecret}")));
        }

        private void SetBearerAuthenticationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", AccessToken);
        }

        private Func<Task<HttpResponseMessage>> CreateRequest(HttpRequestOptions httpRequestOptions)
        {
            var httpClient = _httpClientFactory.CreateClient();
            if (httpRequestOptions.IsOauth)
            {
                SetBearerAuthenticationHeader(httpClient);
            }
            else
            {
                SetBasicAuthenticationHeader(httpClient);
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
                        return httpClient.GetAsync(httpRequestOptions.Uri);
                    });
                case "POST":
                    return new Func<Task<HttpResponseMessage>>(() =>
                    {
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
