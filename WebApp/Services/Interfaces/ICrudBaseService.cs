using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WebApp.Services.Interfaces
{
    public interface ICrudBaseService<T>
    {
        Task<int> Add<TModel>(TModel model);

        Task<List<TModel>> ODataGetList<TModel>(ODataQueryOptions<T> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null);

        Task<List<TModel>> GetList<TModel>(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null);

        Task<TModel> GetById<TModel>(int id, string include = null);

        Task<int> Update<TModel>(int entityId, TModel model, List<string> updatedProperties = null);

        Task<int> Update<TModel>(T baseEntity, TModel model, List<string> updatedProperties = null);

        Task<int> Delete(int id);

        Task<int> Delete(T entity);

        Task<int> DeleteRange(List<T> entities);

        Task<int> AddRange(List<T> entities);

        Task<int> UpdateRange(List<T> entities);

        Task<int> GetCount(string filter = null, Expression<Func<T, bool>> predicate = null);
    }
}