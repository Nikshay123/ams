using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserBaseModel, User>().ReverseMap();
            CreateMap<UserMinModel, User>().ReverseMap();
            CreateMap<UserSelfModel, User>().ReverseMap();
            CreateMap<UserLeafModel, User>().ReverseMap();
            CreateMap<UserModel, User>().ReverseMap();
        }
    }
}