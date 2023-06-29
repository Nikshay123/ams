using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;

namespace WebApp.Data
{
    public class AuditDbContextBase : DbContext, IDbContext
    {
        //Support to serialize and deserialize dates to always be UTC
        public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
        {
            public DateTimeToUtcConverter() : base(Serialize, Deserialize, null)
            {
            }

            private static Expression<Func<DateTime, DateTime>> Deserialize =
                    x => x.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(x, DateTimeKind.Utc) : x;

            private static Expression<Func<DateTime, DateTime>> Serialize = x => x.ToUniversalTime();
        }

        public AuditDbContextBase(DbContextOptions options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Configure ALL the DateTime properties in our EF model so they'll have DateTime.Kind set to Utc.
            // (We'll also have to do this manually for any SQL queries we don't use EFCore to map to objects)
            configurationBuilder
                .Properties<DateTime>()
                .HaveConversion<DateTimeToUtcConverter>();
        }

        private void SetAuditProperties()
        {
            List<EntityEntry> modifiedOrAddedEntities = this.ChangeTracker.Entries()
                          .Where(x => x.State == EntityState.Modified || x.State == EntityState.Added)
                          .Where(x => x.Entity is BaseEntity).ToList();

            foreach (var entry in modifiedOrAddedEntities)
            {
                var entity = entry.Entity as BaseEntity;

                if (entity != null)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = UserContext;
                    }
                    else
                    {
                        entity.ModifiedDatetime = DateTime.Now;
                    }

                    entity.ModifiedBy = UserContext;
                }
            }
        }

        IQueryable<T> IDbContext.Set<T>()
        {
            return base.Set<T>();
        }

        public string UserContext { get; set; }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetAuditProperties();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetAuditProperties();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}