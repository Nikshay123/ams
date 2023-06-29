using AutoMapper;
using WebApp.Data.Entities;

namespace WebApp.Models.Profiles
{
    public class SampleEntityProfile : Profile
    {
        public SampleEntityProfile()
        {
            CreateMap<SampleEntityModel, Sample>().ReverseMap();
        }
    }
}