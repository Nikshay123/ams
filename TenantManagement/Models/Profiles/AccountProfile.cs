using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountBaseModel, Account>().ReverseMap();
            CreateMap<AccountMinModel, Account>().ReverseMap();
            CreateMap<AccountOwnerModel, Account>().ReverseMap();
            CreateMap<AccountLeafModel, Account>().ReverseMap();
            CreateMap<AccountModel, Account>().ReverseMap();
        }
    }
}