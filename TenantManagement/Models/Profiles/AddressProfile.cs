using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<AddressModel, Address>().ReverseMap();
        }
    }
}