using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManager.Data;
using WebApp.Common.Exceptions;
using WebApp.Common.Utils;
using WebApp.Data.Entities;

namespace WebApp.Data.Repositories
{
    public abstract class CrudBaseRepository<T> : BaseRepository, ICrudBaseRepository<T> where T : BaseEntity
    {
        #region Private Properties

        protected readonly ITenantDbContextFactory _contextFactory;
        protected readonly WebAppContext _dbContext;
        protected readonly IRequestContext _requestContext;
        protected readonly ILogger _logger;

        #endregion Private Properties

        #region Constructor

        public CrudBaseRepository(ITenantDbContextFactory contextFactory, IRequestContext requestContext, ILogger logger)
        {
            _contextFactory = contextFactory;
            _dbContext = _contextFactory.DbContext<WebAppContext>();
            _logger = logger;
            _requestContext = requestContext;
        }

        #endregion Constructor

        #region Public Method

        public virtual async Task<int> Add(T entity)
        {
            if (entity == null)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                   { nameof(entity), "null" }
                };

                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                  LogExtensions.GetLogMessage(nameof(Add), paramDict, nameof(entity).GetNullLog()), null, _logger);
            }

            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return GetPrimaryId(entity);
        }

        public virtual async Task<List<T>> ODataGetList(ODataQueryOptions<T> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            var validationSettings = new ODataValidationSettings()
            {
                AllowedFunctions = AllowedFunctions.AllFunctions,
                AllowedQueryOptions = AllowedQueryOptions.All,
            };
            queryOptions.Validate(validationSettings);

            queryable = queryable.AddInclude<T>(_contextFactory.ExcludeCrossDBReferences(include));

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            var querySettings = new ODataQuerySettings()
            {
                EnableConstantParameterization = true,
                EnableCorrelatedSubqueryBuffering = true,
                HandleReferenceNavigationPropertyExpandFilter = true,
                IgnoredQueryOptions = AllowedQueryOptions.Expand | AllowedQueryOptions.Select
            };

            queryable = (IQueryable<T>)queryOptions.ApplyTo(queryable, querySettings);

            var result = await queryable.ToListAsync();
            if (result != null && result.Count > 0)
            {
                await _contextFactory.ResolveCrossDbReferences(include);
            }

            return result;
        }

        public virtual async Task<int> GetCount(string filter = "", System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
        {
            return await (await ListQuery(filter: filter, predicate: predicate)).CountAsync();
        }

        public virtual async Task<List<T>> GetList(string include = null, string filter = "", List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
        {
            var result = await (await ListQuery(_contextFactory.ExcludeCrossDBReferences(include), filter, sort, limit, offset, predicate)).ToListAsync();
            if (result != null && result.Count > 0)
            {
                await _contextFactory.ResolveCrossDbReferences(include);
            }
            return result;
        }

        protected virtual async Task<IQueryable<T>> ListQuery(string include = null, string filter = "", List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            query = query.AddInclude(include);

            query = query.Filter(filter);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (sort != null)
            {
                query = query.Sort<T>(sort);
            }

            if (limit > 0)
            {
                var total = await query.CountAsync();
                _requestContext.PagingCtx = new PagingContext()
                {
                    Total = total,
                    Limit = limit,
                    Offset = offset,
                };

                query = query.Skip(offset).Take(limit);
            }

            return query;
        }

        public virtual async Task<T> GetById(int entityId, string include = null)
        {
            if (entityId <= 0)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                  { nameof(entityId), entityId.ToString() },
                };
                throw new ApiException(ErrorResponse.ErrorEnum.Validation,
                   LogExtensions.GetLogMessage(nameof(GetById), paramDict, nameof(entityId).GetInvalidIntLog()), null, _logger);
            }

            IQueryable<T> query = _dbContext.Set<T>();

            query = query.AddInclude(_contextFactory.ExcludeCrossDBReferences(include));

            var entity = (T)Activator.CreateInstance(typeof(T));
            query = query.Filter<T>($"{GetPrimaryIdPropertyInfo(entity).Name} eq {entityId}");

            var result = await query.FirstOrDefaultAsync();
            if (result != null)
            {
                await _contextFactory.ResolveCrossDbReferences(include);
            }
            return result;
        }

        public virtual async Task<int> Update(T entity)
        {
            if (entity == null)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                   { nameof(entity), "null" }
                };

                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                  LogExtensions.GetLogMessage(nameof(Add), paramDict, nameof(entity).GetNullLog()), null, _logger);
            }

            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();

            return GetPrimaryId(entity);
        }

        public virtual async Task<int> Delete(int entityId)
        {
            if (entityId <= 0)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                  { nameof(entityId), entityId.ToString() },
                };
                throw new ApiException(ErrorResponse.ErrorEnum.Validation,
                   LogExtensions.GetLogMessage(nameof(GetById), paramDict, nameof(entityId).GetInvalidIntLog()), null, _logger);
            }

            var entity = (T)Activator.CreateInstance(typeof(T));
            SetPrimaryId(entity, entityId);
            _dbContext.Set<T>().Attach(entity);
            _dbContext.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> Delete(T entity)
        {
            if (entity == null)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                  { nameof(entity), JsonSerializer.Serialize(entity) },
                };
                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                   LogExtensions.GetLogMessage(nameof(GetById), paramDict, nameof(entity).GetNullOrEmptyLog()), null, _logger);
            }

            _dbContext.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteRange(List<T> entities)
        {
            if (entities == null || entities.Count <= 0)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                   { nameof(entities), JsonSerializer.Serialize(entities) },
                };

                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                LogExtensions.GetLogMessage(nameof(DeleteRange), paramDict, nameof(entities).GetNullOrEmptyLog()), null, _logger);
            }

            _dbContext.RemoveRange(entities);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> AddRange(List<T> entities)
        {
            if (entities == null || entities.Count <= 0)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                   { nameof(entities), JsonSerializer.Serialize(entities) },
                };

                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                LogExtensions.GetLogMessage(nameof(AddRange), paramDict, nameof(entities).GetNullOrEmptyLog()), null, _logger);
            }

            await _dbContext.AddRangeAsync(entities);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateRange(List<T> entities)
        {
            if (entities == null || entities.Count <= 0)
            {
                Dictionary<string, string> paramDict = new Dictionary<string, string>()
                {
                   { nameof(entities), JsonSerializer.Serialize(entities) },
                };

                throw new ApiException(ErrorResponse.ErrorEnum.NullObject,
                LogExtensions.GetLogMessage(nameof(UpdateRange), paramDict, nameof(entities).GetNullOrEmptyLog()), null, _logger);
            }

            _dbContext.UpdateRange(entities);
            return await _dbContext.SaveChangesAsync();
        }

        #endregion Public Method

        #region Private Methods

        protected static PropertyInfo GetPrimaryIdPropertyInfo(T entity)
        {
            var props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    KeyAttribute keyAttr = attr as KeyAttribute;
                    if (keyAttr != null)
                    {
                        return prop;
                    }
                }
            }

            return null;
        }

        protected static int GetPrimaryId(T entity)
        {
            var prop = GetPrimaryIdPropertyInfo(entity);
            if (prop != null)
            {
                var id = prop.GetValue(entity);
                return (int)id;
            }

            return 0;
        }

        protected static void SetPrimaryId(T entity, int id)
        {
            var prop = GetPrimaryIdPropertyInfo(entity);
            if (prop != null)
            {
                prop.SetValue(entity, id);
            }
        }

        protected R Duplicate<S, R>(S sourceEntity)
        {
            var json = JsonSerializer.Serialize(sourceEntity);
            return JsonSerializer.Deserialize<R>(json);
        }

        private bool DoesEntityExist<TEntity>(DbContext dbContext, TEntity entity) where TEntity : class
        {
            var entry = dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(e => e.Entity == entity);
            return entry != null && entry.State != EntityState.Detached;
        }

        public void ClearRelatedEntities(T entity)
        {
            // Get the type of the entity
            var entityType = entity.GetType();

            // Get the properties of the entity
            var entityProperties = entityType.GetProperties();

            // Iterate through the properties
            foreach (var property in entityProperties)
            {
                // Check if the property represents a navigation property
                if (property.PropertyType.IsClass && !property.PropertyType.Namespace.StartsWith("System"))
                {
                    // Exclude the related entity by setting it to null or detaching it
                    var relatedEntity = property.GetValue(entity);
                    if (relatedEntity != null)
                    {
                        property.SetValue(entity, null);
                    }
                }
            }
        }

        public void ClearRelatedEntities(List<T> entities)
        {
            foreach (var entity in entities)
            {
                ClearRelatedEntities(entity);
            }
        }

        #endregion Private Methods
    }
}