using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data;
using TenantManagement.Data.Interfaces;
using TenantManagement.Services;
using TenantManagement.Services.Interfaces;
using WebApp.Services;

namespace TenantManagement.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterTenantServices(this IServiceCollection services)
        {
            //Adding services
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRazorEmailRenderer, RazorTemplateRenderer>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IProfileStorageService, ProfileStorageService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            return services;
        }
    }
}