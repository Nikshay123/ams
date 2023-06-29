using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<Entities.User> GetByIdAsync(int id, string include = null);

        Task<Entities.User> GetByNameAsync(string username, string include = null, bool includeDisabled = true);

        Task<List<User>> GetList(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, Expression<Func<User, bool>> predicate = null, bool includeDisabled = false);

        Task Update(Entities.User user);

        Task CreateUser(Entities.User userData);

        Task Remove(User user);

        Task<List<User>> ODataGetList(ODataQueryOptions<User> queryOptions, string include = null, Expression<Func<User, bool>> predicate = null, bool includeDisabled = false);
    }
}