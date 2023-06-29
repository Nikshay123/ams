using System.Linq;

namespace TenantManagement.Data.Interfaces
{
    public interface IDbContext
    {
        public IQueryable<T> Set<T>() where T : class;
    }
}