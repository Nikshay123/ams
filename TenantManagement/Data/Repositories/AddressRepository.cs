using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;

namespace TenantManagement.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IConfiguration _config;
        private readonly AppGlobalContext _dbcontext;
        private readonly ITenantDbContextFactory _dbtenantfactory;
        private readonly IRequestContext _reqContext;

        public AddressRepository(IConfiguration config, AppGlobalContext dbcontext, ITenantDbContextFactory dbtenantfactory, IRequestContext requestContext)
        {
            _config = config;
            _dbcontext = dbcontext;
            _dbtenantfactory = dbtenantfactory;
            _reqContext = requestContext;

            _dbcontext.UserContext = $"{requestContext.UserId}-{requestContext.Username}";
        }

        public async Task Add(Address address)
        {
            if (address != null)
            {
                await _dbcontext.Addresses.AddAsync(address);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task AddRange(List<Address> addresses)
        {
            if (addresses != null && addresses.Count > 0)
            {
                await _dbcontext.Addresses.AddRangeAsync(addresses);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task Update(Address address)
        {
            if (address != null)
            {
                _dbcontext.Addresses.Update(address);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task UpdateRange(List<Address> addresses)
        {
            if (addresses != null && addresses.Count > 0)
            {
                _dbcontext.Addresses.UpdateRange(addresses);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task Delete(Address address)
        {
            if (address != null)
            {
                _dbcontext.Addresses.Remove(address);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task DeleteRange(List<Address> addresses)
        {
            if (addresses != null && addresses.Count > 0)
            {
                _dbcontext.Addresses.RemoveRange(addresses);
                await _dbcontext.SaveChangesAsync();
            }
        }
    }
}