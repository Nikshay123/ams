using Microsoft.Extensions.DependencyInjection;
using WebApp.Data.Repositories;
using WebApp.Data.Repositories.Interfaces;

namespace WebApp.Data.Extensions
{
    public static class ServiceCollectionRepositoryExtensions
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<ISampleRepository, SampleRepository>();
            return services;
        }
    }
}