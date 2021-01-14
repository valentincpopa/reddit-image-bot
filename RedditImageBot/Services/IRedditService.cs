using RedditImageBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public interface IRedditService
    {
        Task InitializeAsync();
        Task ReplyAsync(string parent, string text);
        Task<IEnumerable<MessageThing>> GetUnreadMessagesAsync();
        Task<PostThing> GetPostAsync(string fullname);
    }
}
