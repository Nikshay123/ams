using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
	internal class AccountConfiguration : BaseConfiguration<Account>
    {
        public override void Configure(EntityTypeBuilder<Account> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Enabled).HasDefaultValue(true);
            builder.Property(x => x.Name).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.Name });
            builder.HasMany(x => x.Users).WithOne(o => o.Account).HasForeignKey(x => x.AccountId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
        }
    }
}