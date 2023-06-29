using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using WebApp.Data.Entities;

namespace WebApp.Data.Configurations
{
    internal class SampleConfiguration : BaseConfiguration<Sample>
    {
        public override void Configure(EntityTypeBuilder<Sample> builder)
        {
            base.Configure(builder);
        }
    }
}