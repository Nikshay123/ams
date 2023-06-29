using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Common.Interfaces
{
    public interface ICommonService
    {
        bool CheckRole(List<Roles> roles, bool accountContext = false);

        string HashPassword(string username, string userpass, string salt, bool legacy = false);

        Task<TModel> GetByIdAsync<TModel>(int id, string include = null);

        Task<TModel> GetByNameAsync<TModel>(string name, string include = null);

        List<Role> ValidateRoleAssignment(List<Role> roles, bool accountRoles = false);
    }
}