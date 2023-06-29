using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class AccountUserProfile : Profile
    {
        public AccountUserProfile()
        {
            CreateMap<AccountUserModel, AccountUser>().ReverseMap();
            CreateMap<AccountUserMinModel, AccountUser>().ReverseMap();
        }
    }
}