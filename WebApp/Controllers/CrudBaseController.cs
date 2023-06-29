using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Models;
using WebApp.Common.Exceptions;
using WebApp.Common.Utils;
using WebApp.Data.Entities;
using WebApp.Services.Interfaces;

namespace WebApp.Controllers
{
    public abstract class CrudBaseController<TEntity> : ControllerBase where TEntity : BaseEntity
    {
        #region Fields

        protected readonly ICrudBaseService<TEntity> _crudBaseService;
        protected readonly ILogger _logger;
        protected readonly IRequestContext _requestContext;

        #endregion Fields

        #region Constructor

        public CrudBaseController(ICrudBaseService<TEntity> crudBaseService, IRequestContext requestContext, ILogger logger)
        {
            _crudBaseService = crudBaseService;
            _logger = logger;
            _requestContext = requestContext;
        }

        #endregion Constructor

        #region Methods

        protected virtual async Task<ActionResult<int>> Add<TModel>(TModel model)
        {
            try
            {
                if (model == null)
                {
                    var paramDict = new Dictionary<string, string>()
                    {
                        { nameof(model), model == null?"null":JsonSerializer.Serialize(model) },
                    };
                    throw new ApiException(ErrorResponse.ErrorEnum.Validation, LogExtensions.GetLogMessage(nameof(Add), paramDict, "Invalid Feedlot Id or Model!!!"), null, _logger);
                }

                return Ok(await _crudBaseService.Add<TModel>(model));
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(model), model == null?"null":JsonSerializer.Serialize(model) },
                };

                _logger.LogErrorExt(ex, nameof(Add), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        protected virtual async Task<ActionResult<List<TModel>>> ODataGetList<TModel>(ODataQueryOptions<TEntity> queryOptions, string include = null)
        {
            return await _crudBaseService.ODataGetList<TModel>(queryOptions, include);
        }

        protected virtual async Task<ActionResult<int>> GetCount(string filter = null)
        {
            try
            {
                return await _crudBaseService.GetCount(filter);
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message, type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(filter), filter },
                };

                _logger.LogErrorExt(ex, nameof(GetCount), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message, type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        protected virtual async Task<ActionResult<Object>> GetList<TModel>(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0)
        {
            try
            {
                var result = await _crudBaseService.GetList<TModel>(include, filter, sort, limit, offset);
                if (_requestContext.PagingCtx != null)
                {
                    return new PagingModel<TModel>(_requestContext.PagingCtx, result);
                }

                return result;
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message, type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(filter), filter },
                    { nameof(sort), JsonSerializer.Serialize(sort) },
                    { nameof(limit), limit.ToString() },
                    { nameof(offset), offset.ToString() },
                };
                _logger.LogErrorExt(ex, nameof(GetList), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message, type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        protected virtual async Task<ActionResult<TModel>> GetById<TModel>(int id, string include = null)
        {
            try
            {
                var entity = await _crudBaseService.GetById<TModel>(id, include);
                if (entity == null)
                {
                    return NotFound();
                }

                return Ok(entity);
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message, type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(id), id.ToString() },
                    { nameof(include), include }
                };

                _logger.LogErrorExt(ex, nameof(Update), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message, type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        protected virtual async Task<ActionResult> Update<TModel>(int id, TModel model)
        {
            try
            {
                return await _crudBaseService.Update<TModel>(id, model, await GetPropertyUpdates()) > 0 ? NoContent() : NotFound();
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message, type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(id), id.ToString() },
                    { nameof(model), JsonSerializer.Serialize(model) }
                };

                _logger.LogErrorExt(ex, nameof(Update), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message, type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        protected virtual async Task<ActionResult> DeleteBase(int id)
        {
            try
            {
                return await _crudBaseService.Delete(id) > 0 ? NoContent() : NotFound();
            }
            catch (ApiException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message, type: ex.ErrCode.ToErrorCodeString());
            }
            catch (Exception ex)
            {
                var paramDict = new Dictionary<string, string>()
                {
                    { nameof(id), id.ToString() }
                };

                _logger.LogErrorExt(ex, nameof(DeleteBase), paramDict, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message, type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Get PATCH Properties to Update
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<List<string>> GetPropertyUpdates()
        {
            if (_requestContext != null && _requestContext.HttpCtx != null)
            {
                var req = _requestContext.HttpCtx.Request;
                req.Body.Position = 0;
                var jsonstr = await new StreamReader(req.Body).ReadToEndAsync();
                req.Body.Position = 0;

                if (!string.IsNullOrEmpty(jsonstr))
                {
                    JObject json = JObject.Parse(jsonstr);
                    return json.Properties().Select(x => x.Name).ToList();
                }
            }

            return new List<string>();
        }

        #endregion Methods
    }
}