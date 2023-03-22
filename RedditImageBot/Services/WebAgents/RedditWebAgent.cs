using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RedditImageBot.Models;
using RedditImageBot.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services.WebAgents
{
    public class RedditWebAgent : WebAgent
    {
        public RedditWebAgent(
            IHttpClientFactory httpClientFactory,
            IOptions<RedditWebAgentConfiguration> webAgentConfiguration,
            ILogger<RedditWebAgent> logger) : base(logger, httpClientFactory, webAgentConfiguration)
        {
        }

        protected override async Task Authenticate()
        {
            var refreshTokenExists = !string.IsNullOrWhiteSpace(_webAgentConfiguration.RefreshToken);
            var formContent = GetAuthenticationFormUrlEncodedContent(refreshTokenExists);

            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpContent = formContent,
                HttpMethod = HttpMethod.Post,
                IsOauth = false,
                Uri = "access_token"
            };

            var request = CreateRequest(httpRequestOptions);
            var response = await request();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(content);

                var lockObj = new object();
                lock (lockObj)
                {
                    if (!refreshTokenExists)
                    {
                        _webAgentConfiguration.RefreshToken = authenticationResponse.RefreshToken;
                    }

                    AccessToken = authenticationResponse.AccessToken;
                }
            }
            else
            {
                await HandleErrors(response);
            }
        }

        protected override void SetRateLimit(HttpResponseMessage response)
        {
            var rateLimitData = new RateLimit
            {
                RemainingRequests = GetNumericHeaderValue(response, "x-ratelimit-remaining"),
                UsedRequests = GetNumericHeaderValue(response, "x-ratelimit-used"),
                TimeUntilReset = GetNumericHeaderValue(response, "x-ratelimit-reset")
            };

            Interlocked.Exchange(ref _rateLimitData, rateLimitData);
        }

        protected override async Task HandleErrors(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
            _logger.LogError("An error occured during the processing of the request: {statusCode} - '{error}'.", response.StatusCode, error != null ? error : "no error message returned");
        }

        private FormUrlEncodedContent GetAuthenticationFormUrlEncodedContent(bool isRefresh)
        {
            if (isRefresh)
            {
                return new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _webAgentConfiguration.RefreshToken)
                });
            }
            else
            {
                return new FormUrlEncodedContent(new[]
                 {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", _webAgentConfiguration.Code),
                    new KeyValuePair<string, string>("redirect_uri", _webAgentConfiguration.RedirectUri)
                });
            }
        }
    }
}
