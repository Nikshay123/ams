using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Configurations
{
    internal class UserConfiguration : BaseConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Enabled).HasDefaultValue(true);
            builder.HasIndex(x => x.Username).IsUnique();
            builder.HasIndex(x => x.Userhash);
            builder.HasIndex(x => x.TransientAuthToken);
            builder.HasIndex(x => x.RefreshToken);
            builder.HasIndex(x => x.TenantId);
            builder.HasMany(x => x.Accounts).WithOne(u => u.User).HasForeignKey(x => x.UserId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
        }
    }
}