using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using TenantManagement.Data.Entities.Interfaces;
using TenantManagement.Data.Interfaces;
using WebApp.Data.Configurations;
using WebApp.Data.Entities;

namespace WebApp.Data
{
    public class WebAppContext : AuditDbContextBase, IAccountReferenceHolder
    {
        public static WebAppContext CreateWithUserContext(DbContextOptions options, string userContext)
        {
            var dbcontext = new WebAppContext((DbContextOptions<WebAppContext>)options);
            dbcontext.UserContext = userContext;
            return dbcontext;
        }

        protected ConcurrentDictionary<int, ConcurrentBag<IAccountHolder>> _accountReferences = new();

        public ConcurrentDictionary<int, ConcurrentBag<IAccountHolder>> AccountReferences
        { get { return _accountReferences; } }

        public DbSet<BaseAttachment> Attachments { get; set; }

        public DbSet<Sample> Samples { get; set; }

        public WebAppContext(DbContextOptions<WebAppContext> options)
            : base(options)
        {
            ChangeTracker.Tracked += ChangeTracker_Tracked;
        }

        //code handles auto loading account between dbcontext (databases) if needed
        protected void ChangeTracker_Tracked(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
        {
            var accountHolder = (e.Entry.Entity as IAccountHolder);
            if (accountHolder != null && accountHolder.AccountId.HasValue)
            {
                var accountId = accountHolder.AccountId;
                if (_accountReferences.ContainsKey(accountId.Value))
                {
                    _accountReferences[accountId.Value].Add(accountHolder);
                }
                else
                {
                    _accountReferences.TryAdd(accountId.Value, new ConcurrentBag<IAccountHolder>() { accountHolder });
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new SampleConfiguration());
        }
    }
}