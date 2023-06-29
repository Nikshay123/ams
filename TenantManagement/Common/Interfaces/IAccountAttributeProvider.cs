using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Common.Interfaces
{
    public interface IAccountAttributeProvider
    {
        Task GetAttributes(IRequestContext requestCtx, Account account, string include);
    }
}