﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Services.WebAgents;
using RedditImageBot.Utilities;
using RedditImageBot.Utilities.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class RedditService : IRedditService
    {
        private readonly RedditWebAgent _redditWebAgent;
        private readonly ILogger<RedditService> _logger;

        public RedditService(RedditWebAgent redditWebAgent, ILogger<RedditService> logger) 
        {
            _redditWebAgent = redditWebAgent;
            _logger = logger;
        }

        public async Task ReplyAsync(string parent, string text)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("parent", parent),
                new KeyValuePair<string, string>("text", text)
            });

            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpContent = content,
                HttpMethod = HttpMethod.Post,
                IsOauth = true,
                Uri = "api/comment",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            if (response == null)
            {
                throw new RedditServiceException($"Could not reply to the message: {parent}.");
            }
        }

        public async Task<IEnumerable<MessageThing>> GetUnreadMessagesAsync()
        {
            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpMethod = HttpMethod.Get,
                IsOauth = true,
                Uri = "message/unread",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            if (response == null)
            {
                throw new RedditServiceException($"Could not get unread messages.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Root<MessageThing>>(content);

            var messages = root.Data.Children.Select(x => x.Data);
            return messages.Where(x => x.IsUsernameMention || x.IsPrivateMessage);
        }

        public async Task<PostThing> GetPostAsync(string fullname)
        {
            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpMethod = HttpMethod.Get,
                IsOauth = true,
                Uri = $"api/info?id={fullname}",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            if (response == null)
            {
                throw new RedditServiceException($"Could not retrieve parent post: {fullname}.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Root<PostThing>>(content);

            var posts = root.Data.Children.Select(x => x.Data);
            return posts.FirstOrDefault();
        }

        public async Task ReadMessagesAsync(string fullname)
        {
            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpMethod = HttpMethod.Post,
                IsOauth = true,
                Uri = $"api/read_message?id={fullname}",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            if (response == null)
            {
                throw new RedditServiceException($"Could not mark message as read: {fullname}.");
            }
        }

        public async Task<List<string>> GetCommentRepliesAuthorsAsync(string postFullName, string commentFullName)
        {
            var httpRequestOptions = new InternalHttpRequestOptions
            {
                HttpMethod = HttpMethod.Get,
                IsOauth = true,
                Uri = $"/comments/{postFullName[3..]}?comment={commentFullName[3..]}",
                Retries = 3,
                Timeout = 10
            };

            var response = await _redditWebAgent.SendRequestAsync(httpRequestOptions);
            if (response == null)
            {
                throw new RedditServiceException($"Could not retrieve comment replies for: {commentFullName}.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<List<Root<ReplyThingWrapper>>>(content);

            return root.Last().Data.Children
                .SelectMany(x => x.Data?.Replies?.Data.Children.Select(x => x.Data?.Author) ?? new List<string>())?
                .ToList();
        }
    }
}
