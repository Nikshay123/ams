using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using WebApp.Data.Entities;
using WebApp.Models;
using WebApp.Services.Interfaces;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SampleController : CrudBaseController<Sample>
    {
        public SampleController(ISampleService StoragePlanService, IRequestContext requestContext, ILogger<SampleController> logger) :
            base(StoragePlanService, requestContext, logger)
        { }

        [AuthorizeRoles(Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult<int>> Add(SampleEntityModel model)
        {
            return await base.Add<SampleEntityModel>(model);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SampleEntityModel>> Get(int id, string include = null)
        {
            return await base.GetById<SampleEntityModel>(id, include);
        }

        /// <summary>
        /// General Get List Of Objects with general support for filtering/paging/sorting
        /// </summary>
        /// <param name="include">comma separated list of related properties to include in the result.  Can include child properties using "."</param>
        /// <param name="filter">semicolon separated list of filter expressions</param>
        /// <param name="sort">comma separated list of properties to sort in order of priority.  a "^" prefix represents desc sort order</param>
        /// <param name="limit">max number of records to retrieve</param>
        /// <param name="offset">starting index into the result of records to return</param>
        /// <returns>if limit value is greater than 0 a Paging Object is return with the paging result and paging info, otherwise the result list is returned</returns>
        [HttpGet()]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetList(string include = null, string filter = null, [FromQuery] List<string> sort = null, int limit = 0, int offset = 0)
        {
            return await base.GetList<SampleEntityModel>(include, filter, sort, limit, offset);
        }

        /// <summary>
        /// General Get List Of Objects with general support for filtering/paging/sorting
        /// </summary>
        /// <param name="include">comma separated list of related properties to include in the result.  Can include child properties using "."</param>
        /// <param name="filter">semicolon separated list of filter expressions</param>
        /// <param name="sort">comma separated list of properties to sort in order of priority.  a "^" prefix represents desc sort order</param>
        /// <param name="limit">max number of records to retrieve</param>
        /// <param name="offset">starting index into the result of records to return</param>
        /// <returns>if limit value is greater than 0 a Paging Object is return with the paging result and paging info, otherwise the result list is returned</returns>
        [HttpGet("Odata")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> OdataGetList(ODataQueryOptions<Sample> queryOptions, string include = null)
        {
            return await base.ODataGetList<SampleEntityModel>(queryOptions, include);
        }

        [AuthorizeRoles(Roles.Admin)]
        [HttpPut("{id}")]
        public async Task<ActionResult<SampleEntityModel>> Update(int id, SampleEntityModel model)
        {
            return await base.Update<SampleEntityModel>(id, model);
        }

        [AuthorizeRoles(Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await base.DeleteBase(id);
        }
    }
}