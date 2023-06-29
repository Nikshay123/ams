using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApp.Data.Entities;

namespace WebApp.Data.Repositories
{
    public interface ICrudBaseRepository<T>
    {
        Task<int> Add(T entity);

        Task<List<T>> ODataGetList(ODataQueryOptions<T> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null);

        Task<List<T>> GetList(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null);

        Task<T> GetById(int entityId, string include = null);

        Task<int> Update(T entity);

        Task<int> Delete(int entityId);

        Task<int> Delete(T entity);

        Task<int> DeleteRange(List<T> entities);

        Task<int> AddRange(List<T> entities);

        Task<int> UpdateRange(List<T> entities);

        Task<int> GetCount(string filter = "", Expression<Func<T, bool>> predicate = null);

        void ClearRelatedEntities(T entity);

        void ClearRelatedEntities(List<T> entities);
    }
}