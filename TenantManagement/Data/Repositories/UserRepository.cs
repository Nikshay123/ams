using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TenantManagement.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _config;
        private readonly AppGlobalContext _dbcontext;
        private readonly ITenantDbContextFactory _dbtenantfactory;
        protected readonly IRequestContext _reqContext;

        public UserRepository(IConfiguration config, AppGlobalContext dbcontext, ITenantDbContextFactory dbtenantfactory, IRequestContext requestContext)
        {
            _config = config;
            _dbcontext = dbcontext;
            _dbtenantfactory = dbtenantfactory;
            _reqContext = requestContext;
            _dbcontext.UserContext = $"{requestContext.UserId}-{requestContext.Username}";
        }

        public async Task<User> GetByNameAsync(string username, string include, bool includeDisabled = true)
        {
            IQueryable<User> query = _dbcontext.Set<User>();
            query = query.AddInclude(include);
            if (include != null && include.ToLower().Contains(nameof(Account).ToLower()))
            {
                query = query.AddInclude($"{nameof(User.Accounts)}.{nameof(Account)}");
            }

            var user = await query.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            if (_reqContext.TenantId == null || _reqContext.TenantId == Guid.Empty ||
                user?.TenantId == _reqContext.TenantId && (includeDisabled || user?.Enabled == true))
            {
                FilterDisabledAccounts(user);
                return user;
            }

            return null;
        }

        public async Task<User> GetByIdAsync(int id, string include)
        {
            IQueryable<User> query = _dbcontext.Set<User>();
            query = query.AddInclude(include);
            if (include != null && include.ToLower().Contains(nameof(Account).ToLower()))
            {
                query = query.AddInclude($"{nameof(User.Accounts)}.{nameof(Account)}");
            }

            var user = await query.Where(x => x.UserId == id).FirstOrDefaultAsync();

            if (_reqContext.TenantId == Guid.Empty || user?.TenantId == _reqContext.TenantId)
            {
                FilterDisabledAccounts(user);
                return user;
            }

            return null;
        }

        public async Task<List<User>> GetList(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<User, bool>> predicate = null, bool includeDisabled = false)
        {
            IQueryable<User> query = _dbcontext.Set<User>();

            query = query.AddInclude(include);
            if (include != null && include.ToLower().Contains(nameof(Account).ToLower()))
            {
                query = query.AddInclude($"{nameof(User.Accounts)}.{nameof(Account)}");
            }

            if (_reqContext.TenantId != null && _reqContext.TenantId != Guid.Empty)
            {
                query = query.Where(u => u.TenantId == _reqContext.TenantId);
            }

            if (!includeDisabled)
            {
                query = query.Where(u => u.Enabled == true);
            }

            query = query.Filter(filter);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (sort != null)
            {
                query = query.Sort<User>(sort);
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

            var result = await query.ToListAsync();
            return FilterDisabledAccounts(result);
        }

        public virtual async Task<List<User>> ODataGetList(ODataQueryOptions<User> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<User, bool>> predicate = null, bool includeDisabled = false)
        {
            IQueryable<User> queryable = _dbcontext.Set<User>();

            var validationSettings = new ODataValidationSettings()
            {
                AllowedFunctions = AllowedFunctions.AllFunctions,
                AllowedQueryOptions = AllowedQueryOptions.All,
            };
            queryOptions.Validate(validationSettings);

            queryable = queryable.AddInclude<User>(include);
            if (include != null && include.ToLower().Contains(nameof(Account).ToLower()))
            {
                queryable = queryable.AddInclude($"{nameof(User.Accounts)}.{nameof(Account)}");
            }

            if (!includeDisabled)
            {
                queryable = queryable.Where(u => u.Enabled == true);
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

            queryable = (IQueryable<User>)queryOptions.ApplyTo(queryable, querySettings);

            var result = await queryable.ToListAsync();
            return FilterDisabledAccounts(result);
        }

        public async Task CreateUser(User user)
        {
            await _dbcontext.Users.AddAsync(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            if (_reqContext.TenantId == null || _reqContext.TenantId == Guid.Empty || user.TenantId == _reqContext.TenantId)
            {
                if (user.TrackedDisabledAccounts != null && user.TrackedDisabledAccounts.Count > 0)
                {
                    //!!!NOTE: AccountUsers shouldn't be managed via the Accounts collection but on the AccountUsers repo directly
                    //The AccountUsers for disabled accounts are filtered after loading from database requiring us to keep track of them
                    //and add them to the original when updating to the User to avoid deleting disabled Account/User associations
                    if (user.Accounts != null)
                    {
                        user.Accounts = user.Accounts.Union(user.TrackedDisabledAccounts).ToList();
                    }
                    else
                    {
                        user.Accounts = user.TrackedDisabledAccounts;
                    }
                }
                _dbcontext.Users.Update(user);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed.");
            }
        }

        public async Task Remove(User user)
        {
            if (_reqContext.TenantId == Guid.Empty || user.TenantId == _reqContext.TenantId)
            {
                _dbcontext.Remove(user);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed.");
            }
        }

        protected User FilterDisabledAccounts(User user)
        {
            if (user != null && user.Accounts != null && user.Accounts.Count > 0)
            {
                //!!!NOTE: AccountUsers should not be managed via the Accounts collection but on the AccountUsers repo directly
                //The AccountUsers are filtered requiring us to keep track of what was loaded with the User from the database
                //and reset them to the original when updating to the User to avoid deleting disabled Account/User associations
                user.TrackedDisabledAccounts = user.Accounts.FindAll(x => x.UserPrimary == true && (x.Account == null || x.Account.Enabled != true));
                user.Accounts = user.Accounts.FindAll(x => x.Account == null || x.Account.Enabled == true);
            }

            return user;
        }

        protected List<User> FilterDisabledAccounts(List<User> users)
        {
            users.ForEach(x => FilterDisabledAccounts(x));
            return users;
        }
    }
}