using AutoMapper;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models.Profiles
{
    public class ScopeProfile : Profile
    {
        public ScopeProfile()
        {
            CreateMap<ScopeModel, Scope>().ReverseMap();
        }
    }
}