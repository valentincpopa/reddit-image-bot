using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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

    public class PostThing
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

}
