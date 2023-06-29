using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Interfaces
{
    public interface IAccountRepository
    {
        Task Add(Account account);

        Task<List<Account>> GetListAsync(List<int> accountIds, string include = null, string filter = "", List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false);

        Task<Account> GetByIdAsync(int id, string include = null);

        Task<Account> GetByNameAsync(string name, string include = null, bool includeDisabled = false);

        Task Update(Account account);

        Task Remove(int accountId);

        Task Remove(Account account);

        Task<List<Account>> ODataGetList(ODataQueryOptions<Account> queryOptions, string include = null, Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false);
    }
}