using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
    internal class TenantConfiguration : BaseConfiguration<Tenant>
    {
        public override void Configure(EntityTypeBuilder<Tenant> builder)
        {
            base.Configure(builder);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}