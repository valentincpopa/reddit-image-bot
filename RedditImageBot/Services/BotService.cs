using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;
        private readonly IRedditService _redditService;
        private readonly IImageService _imageService;
        private readonly IImgurService _imgurService;       
        private readonly IMapper _mapper;
        private readonly ILogger<BotService> _logger;

        private ConcurrentBag<MessageThing> unresolvedMessages = new ConcurrentBag<MessageThing>();

        public BotService(IRedditService redditService, IImageService imageService, IImgurService imgurService, ApplicationDbContext context, ILogger<BotService> logger, IMapper mapper)
        {
            _redditService = redditService;
            _imageService = imageService;
            _imgurService = imgurService;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task InitializeAsync()
        {
            await _redditService.InitializeAsync();
            var timer = new Timer(async (stateInfo) => await GenerateImagesAsync(), null, 0, 10000);
        }

        private async Task GenerateImagesAsync()
        {
            var messages = await _redditService.GetUnreadMessagesAsync();
            var messagesFullNames = messages.Select(x => x.Name);
            var databaseMessages = await _context.Messages.Where(x => messagesFullNames.Contains(x.Fullname)).Select(x => x.Fullname).ToListAsync();
            var unprocessedMessages = messages.Where(x => !databaseMessages.Contains(x.Name));

            await _context.Messages.AddRangeAsync(_mapper.Map<List<Message>>(unprocessedMessages));
            await _context.SaveChangesAsync();

            unresolvedMessages = new ConcurrentBag<MessageThing>(unprocessedMessages);
            Parallel.ForEach(unresolvedMessages, async (unresolvedMessage) =>
            {
                var post = await _redditService.GetPostAsync(unresolvedMessage.ParentId);
                var image = await _imageService.GenerateImageAsync(post.Title, post.Url);

                var link = await _imgurService.UploadImageAsync(image);
                await _redditService.ReplyAsync(unresolvedMessage.Name, link);
                await _redditService.ReadMessageAsync(unresolvedMessage.Name);

                var messageToUpdate = await _context.Messages.FirstOrDefaultAsync(x => x.Fullname == unresolvedMessage.Name);
                messageToUpdate.IsProcessed = true;

                await _context.ProcessedPosts.AddAsync(new ProcessedPost { Fullname = unresolvedMessage.ParentId, ImageUrl = link });
                await _context.SaveChangesAsync();
            });
        }
    }
}
