using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TenantManagement.Data.Interfaces
{
    public interface ITenantDbContextFactory
    {
        T DbContext<T>() where T : DbContext;

        Task ResolveCrossDbReferences(string include = null);

        string ExcludeCrossDBReferences(string include);
    }
}