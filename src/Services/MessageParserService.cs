using RedditImageBot.Models;
using RedditImageBot.Services.Abstractions;
using System.Text.RegularExpressions;

namespace RedditImageBot.Services
{
    public class MessageParserService : IMessageParserService
    {
        private readonly string THING_ID = nameof(THING_ID);
        private readonly string TITLE = nameof(TITLE);
        private readonly string URL = nameof(URL);

        public ParsedMessageBody Parse(string message)
        {
            var pattern = $"\"(?<{URL}>.*\\/comments\\/(?<{THING_ID}>.*?)\\/.*)\"(?<{TITLE}>.*)";

            var regex = new Regex(pattern); 
            var match = regex.Match(message ?? string.Empty);

            if (match.Success)
            {
                return new ParsedMessageBody(match.Groups[TITLE].Value, match.Groups[URL].Value, match.Groups[THING_ID].Value);
            }

            return new ParsedMessageBody();
        }
    }
}
