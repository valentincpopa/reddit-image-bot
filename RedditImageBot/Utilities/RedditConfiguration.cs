using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Utilities
{
    public class RedditConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
}
