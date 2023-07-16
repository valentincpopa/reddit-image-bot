using RedditImageBot.Models;

namespace RedditImageBot.Services.Abstractions
{
    public interface IMessageParserService
    {
        ParsedMessageBody Parse(string message);
    }
}
