using System;

namespace RedditImageBot.Models
{
    public class ParsedMessageBody
    {
        public ParsedMessageBody() { }

        public ParsedMessageBody(string title, string url, string externalPostId)
        {
            Title = title.Trim()[..500];
            ExternalPostId = $"t3_{externalPostId}";
            Url = url;
        }

        public string Title { get; init; }
        public string ExternalPostId { get; init; }
        public string Url { get; init; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(ExternalPostId))
                {
                    return false;
                }

                try
                {
                    var url = new Uri(Url);
                    return url.Host == "www.reddit.com";
                }
                catch (UriFormatException)
                {
                    return false;
                }
            }
        }
    }
}
