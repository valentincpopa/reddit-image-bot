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
                .ForMember(x => x.ExternalId, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.ExternalParentId, opt => opt.MapFrom(x => x.ParentId));

            CreateMap<Message, MessageMetadata>()
                .ForMember(x => x.InternalMessageId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.ExternalMessageId, opt => opt.MapFrom(x => x.ExternalId))
                .ForMember(x => x.ExternalCommentId, opt => opt.MapFrom(x => x.ExternalId))
                .ForMember(x => x.ExternalPostId, opt => opt.MapFrom(x => x.ExternalParentId));
        }
    }
}
