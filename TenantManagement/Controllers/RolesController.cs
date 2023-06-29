using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Models;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService appRoleservice, IRequestContext requestContext, ILogger<RolesController> logger)
        {
            _roleService = appRoleservice;
            _requestContext = requestContext;
            _logger = logger;
        }

        /// <summary>
        /// Gets the Role by the Role's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Role</returns>
        [Authorize]
        [HttpGet("{name}")]
        public async Task<ActionResult<RoleModel>> GetRole(string name)
        {
            try
            {
                var role = await _roleService.GetRole<RoleModel>(null, name);
                if (role == null)
                {
                    return NotFound();
                }

                return Ok(role);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} | name={name}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets the Role by the Role's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Role</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleModel>> GetRole(int id)
        {
            try
            {
                var role = await _roleService.GetRole<RoleModel>(id);
                if (role == null)
                {
                    return NotFound();
                }

                return Ok(role);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} | id={id}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets all the Roles
        /// </summary>
        /// <returns>Roles List</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<RoleModel>> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRoles<RoleModel>(new List<Roles>());
                if (roles == null)
                {
                    return NotFound();
                }

                return Ok(roles);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }
    }
}