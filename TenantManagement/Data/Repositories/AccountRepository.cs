using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManager.Data;

namespace TenantManagement.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IConfiguration _config;
        private readonly AppGlobalContext _dbcontext;
        private readonly ITenantDbContextFactory _dbtenantfactory;
        protected readonly IRequestContext _reqContext;

        public AccountRepository(IConfiguration config, AppGlobalContext dbcontext, ITenantDbContextFactory dbtenantfactory, IRequestContext requestContext)
        {
            _config = config;
            _dbcontext = dbcontext;
            _dbtenantfactory = dbtenantfactory;
            _reqContext = requestContext;
            _dbcontext.UserContext = $"{requestContext.UserId}-{requestContext.Username}";
        }

        public async Task<Account> GetByNameAsync(string name, string include = null, bool includeDisabled = false)
        {
            var account = await _dbcontext.Accounts.AddInclude(include).Where(x => x.Name == name).FirstOrDefaultAsync();
            if (account == null)
            {
                return null;
            }

            if ((_reqContext.TenantId == Guid.Empty || account.TenantId == _reqContext.TenantId) &&
                (includeDisabled || account?.Enabled == true))
            {
                return account;
            }

            return null;
        }

        public async Task<Account> GetByIdAsync(int id, string include = null)
        {
            var account = await _dbcontext.Accounts.AddInclude(include).Where(x => x.AccountId == id).FirstOrDefaultAsync();
            if (account == null)
            {
                return null;
            }

            if (_reqContext.TenantId == Guid.Empty || account.TenantId == _reqContext.TenantId)
            {
                return account;
            }

            return null;
        }

        public async Task Add(Account account)
        {
            if (_reqContext.TenantId == null || _reqContext.TenantId == Guid.Empty ||
                account.TenantId == _reqContext.TenantId)
            {
                await _dbcontext.Accounts.AddAsync(account);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed.");
            }
        }

        public async Task Update(Account account)
        {
            if (_reqContext.TenantId == Guid.Empty || account.TenantId == _reqContext.TenantId)
            {
                _dbcontext.Accounts.Update(account);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed.");
            }
        }

        public async Task<List<Account>> GetListAsync(List<int> accountIds, string include = null, string filter = "", List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false)
        {
            IQueryable<Account> query = _dbcontext.Set<Account>();

            query = query.AddInclude(include);

            if (_reqContext.TenantId != null && _reqContext.TenantId != Guid.Empty)
            {
                query = query.Where(a => a.TenantId == _reqContext.TenantId);
            }

            if (!includeDisabled)
            {
                query = query.Where(a => a.Enabled == true);
            }

            query = query.Filter(filter);

            if (accountIds != null && accountIds.Count > 0)
            {
                query = query.Where(a => accountIds.Contains(a.AccountId));
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (sort != null)
            {
                query = query.Sort<Account>(sort);
            }

            if (limit > 0)
            {
                var total = await query.CountAsync();
                _reqContext.PagingCtx = new PagingContext()
                {
                    Total = total,
                    Limit = limit,
                    Offset = offset,
                };
                query = query.Skip(offset).Take(limit);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<List<Account>> ODataGetList(ODataQueryOptions<Account> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false)
        {
            IQueryable<Account> queryable = _dbcontext.Set<Account>();

            var validationSettings = new ODataValidationSettings()
            {
                AllowedFunctions = AllowedFunctions.AllFunctions,
                AllowedQueryOptions = AllowedQueryOptions.All,
            };
            queryOptions.Validate(validationSettings);

            queryable = queryable.AddInclude<Account>(include);

            if (!includeDisabled)
            {
                queryable = queryable.Where(a => a.Enabled == true);
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            var querySettings = new ODataQuerySettings()
            {
                EnableConstantParameterization = true,
                EnableCorrelatedSubqueryBuffering = true,
                HandleReferenceNavigationPropertyExpandFilter = true,
                IgnoredQueryOptions = AllowedQueryOptions.Expand | AllowedQueryOptions.Select
            };

            queryable = (IQueryable<Account>)queryOptions.ApplyTo(queryable, querySettings);

            return await queryable.ToListAsync();
        }

        public async Task Remove(int accountId)
        {
            var account = new Account { AccountId = accountId };
            _dbcontext.Attach(account);
            await Remove(account);
        }

        public async Task Remove(Account account)
        {
            if (_reqContext.TenantId == Guid.Empty || account.TenantId == _reqContext.TenantId)
            {
                _dbcontext.Remove(account);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed.");
            }
        }
    }
}