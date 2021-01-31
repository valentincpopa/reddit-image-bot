using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Utilities;
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
        private readonly ThreadingConfiguration _options;

        public BotService(
            IServiceScopeFactory scopeFactory,
            IRedditService redditService,
            IImageService imageService,
            IImgurService imgurService,
            IConfiguration configuration,
            ILogger<BotService> logger,
            IOptions<ThreadingConfiguration> options,
            IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _redditService = redditService;
            _imageService = imageService;
            _imgurService = imgurService;
            _configuration = configuration;
            _options = options.Value;
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

            ProcessImagesInParallel(unprocessedMessages);

            context.Dispose();
        }

        private void ProcessImagesInParallel(IEnumerable<MessageThing> unprocessedMessages)
        {
            _logger.LogInformation("Started processing unread messages.");
            var tasks = new List<Task>();
            var exceptions = new ConcurrentQueue<Exception>();
            var semaphore = new SemaphoreSlim(0, _options.MaxThreadCount);
            foreach (var message in unprocessedMessages)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    var context = GetApplicationDbContext();
                    try
                    {
                        var link = "";
                        var post = await _redditService.GetPostAsync(message.ParentId);
                        var processedPost = await context.ProcessedPosts.FirstOrDefaultAsync(x => x.Fullname == message.ParentId);
                        if (processedPost != null)
                        {
                            link = processedPost.ImageUrl;
                        }
                        else
                        {
                            var image = await _imageService.GenerateImageAsync(post.Title, post.Url);
                            link = await _imgurService.UploadImageAsync(image);

                            processedPost = new ProcessedPost { Fullname = message.ParentId, ImageUrl = link };
                            await context.ProcessedPosts.AddAsync(processedPost);
                            await context.SaveChangesAsync();
                        }

                        var messageToUpdate = await context.Messages.FirstOrDefaultAsync(x => x.Fullname == message.Name);
                        messageToUpdate.IsProcessed = true;
                        messageToUpdate.PostId = processedPost.Id;
                        await context.SaveChangesAsync();

                        await _redditService.ReplyAsync(message.Name, link);
                        await _redditService.ReadMessageAsync(message.Name);
                    }
                    catch (Exception exception)
                    {
                        exceptions.Enqueue(exception);
                    }
                    finally
                    {
                        semaphore.Release();
                        context.Dispose();
                    }
                }));
            }

            semaphore.Release(_options.MaxThreadCount);
            Task.WaitAll(tasks.ToArray());
            _logger.LogInformation("Ended processing unread messages.");

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
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
