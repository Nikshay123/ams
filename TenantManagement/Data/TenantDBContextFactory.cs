using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;

namespace TenantManagement.Data
{
    public class TenantDbContextFactory : ITenantDbContextFactory, IDisposable, IAsyncDisposable
    {
        private readonly string TENANT_CONNECTIONSTRING_NAME = "TenantDatabase";
        private readonly string TENANT_CONTEXT_FACTORY_METHOD_NAME = "Create";
        private readonly string TENANT_CONTEXT_WITH_USER_CONTEXT_FACTORY_METHOD_NAME = "CreateWithUserContext";
        private readonly AppGlobalContext _appDbContext;
        private readonly IConfiguration _config;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<TenantDbContextFactory> _logger;
        private Dictionary<Type, DbContext> _tenantContext = new();

        public TenantDbContextFactory(AppGlobalContext appContext, IConfiguration config, IRequestContext requestContext, ILogger<TenantDbContextFactory> logger)
        {
            _appDbContext = appContext;
            _config = config;
            _requestContext = requestContext;
            _logger = logger;
        }

        public T DbContext<T>() where T : DbContext
        {
            if (_tenantContext.ContainsKey(typeof(T)))
            {
                return (T)Convert.ChangeType(_tenantContext[typeof(T)], typeof(T));
            }

            if (_requestContext.TenantId != null)
            {
                if (_requestContext.TenantId == Guid.Empty)
                {
                    return null;
                }

                var optionsBuilder = new DbContextOptionsBuilder<T>();
                var connectionString = _config.GetConnectionString(TENANT_CONNECTIONSTRING_NAME);
                optionsBuilder.UseSqlServer(string.Format(connectionString, _requestContext.TenantId, GenerateTenantDbPass(_requestContext.TenantId.Value)));
                var createMethod = typeof(T).GetMethod(TENANT_CONTEXT_WITH_USER_CONTEXT_FACTORY_METHOD_NAME, BindingFlags.Public | BindingFlags.Static);
                if (createMethod != null)
                {
                    _tenantContext[typeof(T)] = (DbContext)createMethod.Invoke(null, new object[] { optionsBuilder.Options, $"{_requestContext.UserId.ToString()}-{_requestContext.Username}" });
                }
                else
                {
                    _tenantContext[typeof(T)] = (DbContext)typeof(T).GetMethod(TENANT_CONTEXT_FACTORY_METHOD_NAME, BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { optionsBuilder.Options });
                }
            }
            else
            {
                //Allow null context - needed for hangfire scheduling
                //throw new ArgumentNullException("Invalid Tenant");
                _logger.LogWarning("Invalid TenantId - retuning null");
                return default(T);
            }

            return (T)Convert.ChangeType(_tenantContext[typeof(T)], typeof(T));
        }

        public async Task ResolveCrossDbReferences(string include = null)
        {
            foreach (var dbctx in _tenantContext.Values)
            {
                var accountReferenceHolder = dbctx as IAccountReferenceHolder;
                if (accountReferenceHolder != null)
                {
                    await _appDbContext.ResolveAccounts(accountReferenceHolder.AccountReferences, include);
                }
            }
        }

        public string ExcludeCrossDBReferences(string include)
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

                if (i > 0)
                {
                    includeResult.Add(String.Join('.', subparts.Take(i)));
                }
            }

            if (includeResult.Count > 0)
            {
                return String.Join(',', includeResult);
            }

            return null;
        }

        protected string GenerateTenantDbPass(Guid tenant)
        {
            var secret = _config["AppSecret"];
            return "Tenant:" + CryptoUtils.GenerateHash($"{tenant}:{secret}");
        }

        void IDisposable.Dispose()
        {
            if (_tenantContext != null)
            {
                foreach (var context in _tenantContext)
                {
                    context.Value.Dispose();
                }

                _tenantContext.Clear();
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_tenantContext != null)
            {
                var tc = _tenantContext;
                _tenantContext = null;

                foreach (var context in tc)
                {
                    context.Value.DisposeAsync();
                }
            }

            return default(ValueTask);
        }
    }
}