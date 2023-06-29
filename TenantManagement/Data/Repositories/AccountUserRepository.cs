using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManager.Data;

namespace TenantManagement.Data.Repositories
{
    public class AccountUserRepository : IAccountUserRepository
    {
        private readonly IConfiguration _config;
        private readonly AppGlobalContext _dbcontext;
        private readonly ITenantDbContextFactory _dbtenantfactory;
        protected readonly IRequestContext _reqContext;

        public AccountUserRepository(IConfiguration config, AppGlobalContext dbcontext, ITenantDbContextFactory dbtenantfactory, IRequestContext requestContext)
        {
            _config = config;
            _dbcontext = dbcontext;
            _dbtenantfactory = dbtenantfactory;
            _reqContext = requestContext;
            _dbcontext.UserContext = $"{requestContext.UserId}-{requestContext.Username}";
        }

        public async Task<List<AccountUser>> GetByAsync(int? accountId, int? userId, string include = null)
        {
            IQueryable<AccountUser> query = _dbcontext.AccountUsers.AddInclude(include);

            if (accountId != null)
            {
                query = query.Where(x => x.AccountId == accountId.Value);
            }

            if (userId != null)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task Add(AccountUser AccountUser)
        {
            await _dbcontext.AccountUsers.AddAsync(AccountUser);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task Update(AccountUser AccountUser)
        {
            _dbcontext.AccountUsers.Update(AccountUser);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task Delete(AccountUser AccountUser)
        {
            _dbcontext.AccountUsers.Remove(AccountUser);
            await _dbcontext.SaveChangesAsync();
        }
    }
}