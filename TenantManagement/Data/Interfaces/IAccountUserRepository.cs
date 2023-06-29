using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Interfaces
{
    public interface IAccountUserRepository
    {
        Task Add(AccountUser AccountUser);
        Task Delete(AccountUser AccountUser);
        Task<List<AccountUser>> GetByAsync(int? accountId, int? userId, string include = null);
        Task Update(AccountUser AccountUser);
    }
}