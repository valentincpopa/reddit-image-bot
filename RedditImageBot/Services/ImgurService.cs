using AutoMapper.Configuration;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class ImgurService : IImgurService
    {
        private readonly ImgurConfiguration _options;
        private readonly ILogger<ImgurService> _logger;

        public ImgurService(IOptions<ImgurConfiguration> options, ILogger<ImgurService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        public async Task<string> UploadImageAsync(MemoryStream fileStream)
        {
            var apiClient = new ApiClient(_options.ClientId);
            var httpClient = new HttpClient();

            var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
            fileStream.Seek(0, SeekOrigin.Begin);
            var imageUpload = await imageEndpoint.UploadImageAsync(fileStream);
            await fileStream.DisposeAsync();
            return imageUpload.Link;
        }
    }
}
