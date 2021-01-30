using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedditImageBot.Database;
using RedditImageBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public class BotService : IBotService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRedditService _redditService;
        private readonly IImageService _imageService;
        private readonly IImgurService _imgurService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<BotService> _logger;

        private ConcurrentBag<MessageThing> unresolvedMessages = new ConcurrentBag<MessageThing>();

        public BotService(
            IServiceScopeFactory scopeFactory, 
            IRedditService redditService, 
            IImageService imageService, 
            IImgurService imgurService, 
            IConfiguration configuration, 
            ILogger<BotService> logger, 
            IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _redditService = redditService;
            _imageService = imageService;
            _imgurService = imgurService;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task InitializeAsync()
        {
            await _redditService.InitializeAsync();            
        }

        public async Task GenerateImagesAsync()
        {
            var context = GetApplicationDbContext();
            var messages = await _redditService.GetUnreadMessagesAsync();
            var messagesFullNames = messages.Select(x => x.Name);
            var databaseMessages = await context.Messages.Where(x => messagesFullNames.Contains(x.Fullname)).Select(x => x.Fullname).ToListAsync();
            var unprocessedMessages = messages.Where(x => !databaseMessages.Contains(x.Name));

            await context.Messages.AddRangeAsync(_mapper.Map<List<Message>>(unprocessedMessages));
            await context.SaveChangesAsync();

            unresolvedMessages = new ConcurrentBag<MessageThing>(unprocessedMessages);
            try
            {
                ProcessImagesInParallelAsync();
            }
            catch (AggregateException exceptions)
            {
                foreach (var exception in exceptions.Flatten().InnerExceptions)
                {
                    _logger.LogError(exception.Message);
                }
            }
            context.Dispose();
        }

        private void ProcessImagesInParallelAsync()
        {
            Parallel.ForEach(unresolvedMessages, async (unresolvedMessage) =>
            {
                _logger.LogInformation("Started processing unread messages.");                
                var context = GetApplicationDbContext();

                var link = "";
                var post = await _redditService.GetPostAsync(unresolvedMessage.ParentId);
                var processedPost = await context.ProcessedPosts.FirstOrDefaultAsync(x => x.Fullname == unresolvedMessage.ParentId);
                if (processedPost != null)
                {
                    link = processedPost.ImageUrl;
                }
                else
                {
                    var image = await _imageService.GenerateImageAsync(post.Title, post.Url);
                    link = await _imgurService.UploadImageAsync(image);

                    processedPost = new ProcessedPost { Fullname = unresolvedMessage.ParentId, ImageUrl = link };
                    await context.ProcessedPosts.AddAsync(processedPost);
                    await context.SaveChangesAsync();
                }

                var messageToUpdate = await context.Messages.FirstOrDefaultAsync(x => x.Fullname == unresolvedMessage.Name);
                messageToUpdate.IsProcessed = true;
                messageToUpdate.PostId = processedPost.Id;
                await context.SaveChangesAsync();

                await _redditService.ReplyAsync(unresolvedMessage.Name, link);
                await _redditService.ReadMessageAsync(unresolvedMessage.Name);
                _logger.LogInformation("Ended processing unread messages.");
                context.Dispose();
            });
        }

        private ApplicationDbContext GetApplicationDbContext()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(_configuration.GetConnectionString("Heroku"));

            var context = new ApplicationDbContext(builder.Options);
            return context;
        }
    }
}
