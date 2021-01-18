using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Database
{
    public class ProcessedPost
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
