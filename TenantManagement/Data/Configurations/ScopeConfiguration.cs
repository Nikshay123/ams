using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
    internal class ScopeConfiguration : BaseConfiguration<Scope>
    {
        public override void Configure(EntityTypeBuilder<Scope> builder)
        {
            base.Configure(builder);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}