using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Common.Interfaces;
using WebApp.Services;
using WebApp.Services.Interfaces;

namespace WebApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IAccountAttributeProvider, AccountAttributeProvider>();
            services.AddScoped<ISampleService, SampleService>();
            return services;
        }
    }
}