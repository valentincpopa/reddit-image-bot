using RedditImageBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedditImageBot.Services.Abstractions
{
    public interface IRedditService
    {
        Task ReplyAsync(string parent, string text);
        Task ReadMessagesAsync(string fullname);
        Task<IEnumerable<MessageThing>> GetUnreadMessagesAsync();
        Task<PostThing> GetPostAsync(string fullname);
        Task<List<string>> GetCommentRepliesAuthorsAsync(string postFullName, string commentFullName);
    }
}
