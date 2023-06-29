using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using TenantManagement.Data.Configurations;
using TenantManagement.Data.Entities;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TenantManagement.Data.Entities.Interfaces;
using System.Linq;
using TenantManager.Data;
using System.Collections.Generic;
using System;

namespace TenantManagement.Data
{
    public class AppGlobalContext : AuditDbContextBase
    {
        public AppGlobalContext(DbContextOptions<AppGlobalContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Scope> Scopes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AccountUser> AccountUsers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public async Task ResolveAccounts(ConcurrentDictionary<int, ConcurrentBag<IAccountHolder>> accountReferences, string include = null)
        {
            if (accountReferences.Count > 0 && include != null && include.Contains(nameof(Account)))
            {
                var accountIds = accountReferences.Keys.ToList();
                var accounts = await Accounts.AddInclude(NoramalizeInclude(include)).Where(a => accountIds.Contains(a.AccountId)).ToListAsync();
                var accountMap = accounts.ToDictionary(a => a.AccountId);

                accountReferences.Values.ToList().ForEach(ar =>
                {
                    foreach (var ah in ar)
                    {
                        ah.Account = accountMap[ah.AccountId.Value];
                    }
                });
                accountReferences.Clear();
            }
        }

        protected string NoramalizeInclude(string include)
        {
            if (string.IsNullOrEmpty(include) || !include.Contains(nameof(Account)))
            {
                return include;
            }

            var includeResult = new List<string>();
            var includeParts = include.Split(',');

            foreach (var part in includeParts)
            {
                var subparts = part.Split(".");
                int i = 0;
                for (; i < subparts.Length; i++)
                {
                    if (subparts[i].ToLower() == nameof(Account).ToLower())
                    {
                        break;
                    }
                }

                if (i < subparts.Length - 1)
                {
                    includeResult.Add(String.Join('.', subparts.Skip(i + 1)));
                }
            }

            if (includeResult.Count > 0)
            {
                return String.Join(',', includeResult);
            }

            return null;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new ScopeConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AccountUserConfiguration());
            modelBuilder.ApplyConfiguration(new AddressConfiguration());
        }
    }
}