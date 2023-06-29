using System.Collections.Concurrent;
using System.Linq;
using TenantManagement.Data.Entities.Interfaces;

namespace TenantManagement.Data.Interfaces
{
    public interface IAccountReferenceHolder
    {
        ConcurrentDictionary<int, ConcurrentBag<IAccountHolder>> AccountReferences { get; }
    }
}