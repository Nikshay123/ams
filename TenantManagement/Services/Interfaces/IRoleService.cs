using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Data.Entities;

namespace TenantManagement.Services.Interfaces
{
    public interface IRoleService
    {
        Task DeleteRole(Role role);

        Task<T> GetRole<T>(int? roleId = null, string name = null);

        Task<List<T>> GetRoles<T>(List<int> roleIds = null);

        Task<List<T>> GetRoles<T>(List<Roles> roles = null);
        Task<List<T>> GetRoles<T>(List<string> roles = null);
    }
}