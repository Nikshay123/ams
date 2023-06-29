using AutoMapper;
using WebApp.Data.Entities;

namespace WebApp.Models.Profiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<AttachmentModel, BaseAttachment>().ReverseMap();
        }
    }
}