using Microsoft.Extensions.Logging;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Interfaces;
using WebApp.Data.Entities;
using WebApp.Data.Repositories.Interfaces;

namespace WebApp.Data.Repositories
{
    public class SampleRepository : CrudBaseRepository<Sample>, ISampleRepository
    {
        public SampleRepository(ITenantDbContextFactory contextFactory, IRequestContext requestContext, ILogger<SampleRepository> logger) :
            base(contextFactory, requestContext, logger)
        {
        }
    }
}