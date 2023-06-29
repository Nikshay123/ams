using WebApp.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantManagement.Data.Configuration;

namespace WebApp.Data.Configurations
{
    internal class AttachmentConfiguration : BaseConfiguration<BaseAttachment>
    {
        public override void Configure(EntityTypeBuilder<BaseAttachment> builder)
        {
            base.Configure(builder);
            builder.HasIndex(x => x.PrincipalId);
            builder.HasIndex(c => c.Discriminator);
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.AccessToken);
        }
    }
}