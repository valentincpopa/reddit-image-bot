using AutoMapper;
using RedditImageBot.Database;
using RedditImageBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditImageBot.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<MessageThing, Message>()
                .ForMember(x => x.Fullname, opt => opt.MapFrom(x => x.Name));
        }
    }
}
