using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
    internal class AccountUserConfiguration : BaseConfiguration<AccountUser>
    {
        public override void Configure(EntityTypeBuilder<AccountUser> builder)
        {
            base.Configure(builder);
            builder.HasKey(x => new { x.AccountId, x.UserId });
        }
    }
}