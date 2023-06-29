using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TenantManagement.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        bool RegisterJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan? delay = null, string name = null);

        bool Dispatch();
    }
}