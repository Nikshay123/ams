using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
    internal class RoleConfiguration : BaseConfiguration<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}