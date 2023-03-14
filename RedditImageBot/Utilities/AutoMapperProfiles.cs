using AutoMapper;
using RedditImageBot.Database;
using RedditImageBot.Models;

namespace RedditImageBot.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<MessageThing, Message>()
                .ForMember(x => x.ExternalId, opt => opt.MapFrom(x => x.Name));

            CreateMap<Message, MessageMetadata>()
                .ForMember(x => x.MessageId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.ExternalId, opt => opt.MapFrom(x => x.ExternalId))
                .ForMember(x => x.ExternalCommentId, opt => opt.MapFrom(x => x.ExternalId));
        }
    }
}
