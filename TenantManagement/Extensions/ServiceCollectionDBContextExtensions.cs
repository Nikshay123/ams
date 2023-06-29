using TenantManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TenantManagement.Extensions
{
    public static class ServiceCollectionDBContextExtensions
    {
        public static IServiceCollection RegisterAppGlobalDBContexts(this IServiceCollection services, string connectionString)
        {
            //TODO: Uncomment to support JWT token generation and User management
            services.AddDbContextPool<AppGlobalContext>(options => options
                .UseSqlServer(connectionString)
            );

            return services;
        }
    }
}