using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Controllers;

namespace TenantManagement.Extensions
{
    public static class ServiceCollectionControllerExtensions
    {
        public static void RegisterTenantControllers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AuthController>();
            serviceCollection.AddScoped<RolesController>();
            serviceCollection.AddScoped<UsersController>();
            serviceCollection.AddScoped<AccountsController>();
        }
    }
}