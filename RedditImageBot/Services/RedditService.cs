using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedditImageBot.Models;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class RedditService : IRedditService
    {
        private readonly RedditWebAgent _redditWebAgent;
        private readonly ILogger<RedditService> _logger;
        private readonly IMapper _mapper;

        public RedditService(RedditWebAgent redditWebAgent, ILogger<RedditService> logger, IMapper mapper)
        {
            _redditWebAgent = redditWebAgent;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task InitializeAsync()
        {
            await _redditWebAgent.Initialize();
        }

        public async Task ReplyAsync(string parent, string text)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("parent", parent),
                new KeyValuePair<string, string>("text", text)
            });

            var httpRequestOptions = new HttpRequestOptions
            {
                HttpContent = content,
                HttpMethod = HttpMethod.Post,
                IsOauth = true,
                Uri = "api/comment",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            //TODO: log the response
        }

        public async Task<IEnumerable<MessageThing>> GetUnreadMessagesAsync()
        {
            var httpRequestOptions = new HttpRequestOptions
            {
                HttpMethod = HttpMethod.Get,
                IsOauth = true,
                Uri = "message/unread",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            var content = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Root<MessageThing>>(content);

            var messages = root.Data.Children.Select(x => x.Data);
            return messages;
        }

        public async Task<PostThing> GetPostAsync(string fullname)
        {
            var httpRequestOptions = new HttpRequestOptions
            {
                HttpMethod = HttpMethod.Get,
                IsOauth = true,
                Uri = $"api/info?id={fullname}",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            var content = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Root<PostThing>>(content);

            var posts = root.Data.Children.Select(x => x.Data);
            return posts.FirstOrDefault();
        }
    }
}
