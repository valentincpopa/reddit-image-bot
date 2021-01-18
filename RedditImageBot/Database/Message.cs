using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Database
{
    public class Message
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public bool IsProcessed { get; set; }
        public int PostId { get; set; }
        public ProcessedPost Post { get; set; }
    }
}
