using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;

namespace TenantManagement.Data.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IConfiguration _config;
        private readonly AppGlobalContext _dbcontext;
        private readonly ITenantDbContextFactory _dbtenantfactory;
        private readonly IRequestContext _reqContext;

        public RoleRepository(IConfiguration config, AppGlobalContext dbcontext, ITenantDbContextFactory dbtenantfactory, IRequestContext requestContext)
        {
            _config = config;
            _dbcontext = dbcontext;
            _dbtenantfactory = dbtenantfactory;
            _reqContext = requestContext;

            _dbcontext.UserContext = $"{requestContext.UserId}-{requestContext.Username}";
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            return await _dbcontext.Roles.Where(x => x.Name == (Roles)Enum.Parse(typeof(Roles), name) && x.RoleId > 1).FirstOrDefaultAsync();
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await _dbcontext.Roles.Where(x => x.RoleId == id && x.RoleId > 1).FirstOrDefaultAsync();
        }

        public async Task<List<Role>> GetRolesAsync(List<int> roles = null)
        {
            if (roles != null && roles.Count > 0)
            {
                return await _dbcontext.Roles.Where(r => roles.Contains(r.RoleId) && r.RoleId > 1).ToListAsync();
            }

            return await _dbcontext.Roles.Where(r => r.RoleId > 1).ToListAsync();
        }

        public async Task<List<Role>> GetRolesAsync(List<Roles> roles = null)
        {
            if (roles != null && roles.Count > 0)
            {
                return await _dbcontext.Roles.Where(r => roles.Contains(r.Name) && r.RoleId > 1).ToListAsync();
            }

            return await _dbcontext.Roles.Where(r => r.RoleId > 1).ToListAsync();
        }

        public async Task Add(Role role)
        {
            await _dbcontext.Roles.AddAsync(role);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task Delete(Role role)
        {
            _dbcontext.Roles.Remove(role);
            await _dbcontext.SaveChangesAsync();
        }
    }
}