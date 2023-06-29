using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using WebApp.Controllers;
using WebApp.Data.Entities;

namespace WebApp.Extensions
{
    public static class ServiceCollectionControllerExtensions
    {
        public static void RegisterControllers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddControllers()
                .AddOData(options => options
                    .Select()
                    .Filter()
                    .OrderBy()
                    .SetMaxTop(20)
                    .Count()
                    .Expand()
                );
        }
    }
}