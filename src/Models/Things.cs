using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RedditImageBot.Models
{
    public class Root<T>
    {
        public string Kind { get; set; }
        public Data<T> Data { get; set; }
    }

    public class Data<T>
    {
        public Child<T>[] Children { get; set; }
    }

    public class Child<T>
    {
        public string Kind { get; set; }
        public T Data { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class MessageThing : IEquatable<MessageThing>
    {
        public string Name { get; set; }
        public string Context { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public bool WasComment { get; set; }

        public bool Equals([AllowNull] MessageThing other)
        {
            return null != other && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageThing);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class PostThing
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool Archived { get; set; }
        public bool IsSelf { get; set; }
        public bool IsVideo { get; set; }
        public string PostHint { get; set; }

        public bool IsRedditImage
        {
            get => this.PostHint == "image" && !this.IsVideo;
        }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReplyThingWrapper
    {
        public Root<ReplyThing> Replies { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReplyThing
    {
        public string Author { get; set; }
    }
}
