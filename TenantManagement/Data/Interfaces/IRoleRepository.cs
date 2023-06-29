using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Interfaces
{
    public interface IRoleRepository
    {
        Task Add(Role role);

        Task<Role> GetByIdAsync(int id);

        Task<Role> GetByNameAsync(string name);

        Task<List<Role>> GetRolesAsync(List<int> roles = null);

        Task<List<Role>> GetRolesAsync(List<Roles> roles = null);

        Task Delete(Role role);
    }
}