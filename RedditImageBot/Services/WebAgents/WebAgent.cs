using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Utilities;
using RedditImageBot.Utilities.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services.WebAgents
{
    public abstract class WebAgent : IWebAgent
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger<WebAgent> _logger;
        protected readonly WebAgentConfiguration _webAgentConfiguration;

        protected int authenticating;
        protected RateLimit _rateLimitData;

        protected string AccessToken { get; set; }

        public WebAgent(
            ILogger<WebAgent> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<WebAgentConfiguration> webAgentConfiguration)
        {
            _rateLimitData = new RateLimit();
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _webAgentConfiguration = webAgentConfiguration.Value;
        }

        public RateLimit RateLimitData => _rateLimitData;

        public async Task Initialize()
        {
            await Authenticate();
        }

        public async Task<HttpResponseMessage> SendRequestAsync(InternalHttpRequestOptions httpRequestOptions)
        {
            var request = CreateRequest(httpRequestOptions);
            HttpResponseMessage response = null;
            var shouldRefreshHttpClient = false;

            for (int i = 1; i <= httpRequestOptions.Retries; i++)
            {
                _logger.LogInformation("Attempt [{tryNo} out of {noTries}]..", i, httpRequestOptions.Retries);
                await ProcessRateLimit();

                if (shouldRefreshHttpClient)
                {
                    request = CreateRequest(httpRequestOptions);
                    shouldRefreshHttpClient = false;
                }

                var requestTask = request();
                response = await requestTask;

                if (response.IsSuccessStatusCode)
                {
                    SetRateLimit(response);
                    return response;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    shouldRefreshHttpClient = true;
                    var authenticatingInProgress = false;
                    try
                    {
                        authenticatingInProgress = Interlocked.CompareExchange(ref authenticating, 1, 0) != 0;
                        if (authenticatingInProgress)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        await Authenticate();
                        continue;
                    }
                    finally
                    {
                        if (authenticatingInProgress)
                        {
                            Interlocked.Exchange(ref authenticating, 0);
                        }
                    }
                }
                else
                {
                    SetRateLimit(response);
                    await HandleErrors(response);
                }
                await Task.Delay(3000);
            }
            return null;
        }

        protected int? GetNumericHeaderValue(HttpResponseMessage response, string headerName)
        {
            var headerExists = response.Headers.TryGetValues(headerName, out IEnumerable<string> headerValues);
            if (headerExists)
            {
                return (int)Convert.ToDouble(headerValues.First());
            }
            return null;
        }

        private async Task ProcessRateLimit()
        {
            if (_rateLimitData.RemainingRequests == 0)
            {
                await Task.Delay(_rateLimitData.TimeUntilReset.Value * 1000);
                _logger.LogInformation("Rate limit hit. Waiting for: {seconds} seconds.", _rateLimitData.TimeUntilReset.Value);
            }
        }

        private void SetBasicAuthenticationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{_webAgentConfiguration.ClientId}:{_webAgentConfiguration.ClientSecret}")));
        }

        private void SetBearerAuthenticationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AccessToken);
        }

        protected Func<Task<HttpResponseMessage>> CreateRequest(InternalHttpRequestOptions httpRequestOptions)
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

            var uriBuilder = new UriBuilder(httpRequestOptions.IsOauth ? _webAgentConfiguration.OauthUrl : _webAgentConfiguration.Url);

            httpClient.BaseAddress = uriBuilder.Uri;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                string.Format(_webAgentConfiguration.UserAgent, Assembly.GetExecutingAssembly().GetName().Version));

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
                    throw new InvalidOperationException($"No handler was defined for the requested HTTP method {httpRequestOptions.HttpMethod.Method}.");
            }
        }

        protected abstract Task Authenticate();

        protected abstract void SetRateLimit(HttpResponseMessage response);

        protected abstract Task HandleErrors(HttpResponseMessage response);
    }
}
