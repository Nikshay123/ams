using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Data.Interfaces;
using TenantManagement.Data.Repositories;
using WebApp.Data.Repositories;

namespace TenantManagement.Extensions
{
    public static class RepositoryCollectionExtensions
    {
        public static IServiceCollection RegisterTenantRepositories(this IServiceCollection services)
        {
            //TODO: Uncomment to support JWT token generation and User management
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountUserRepository, AccountUserRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IProfileStorageRepository, ProfileStorageRepository>();
            return services;
        }
    }
}