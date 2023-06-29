using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleModel, Role>().ReverseMap();
        }
    }
}