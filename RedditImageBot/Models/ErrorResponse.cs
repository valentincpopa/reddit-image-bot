using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Models
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"Error: {Error} Message: {Message}";
        }
    }
}
