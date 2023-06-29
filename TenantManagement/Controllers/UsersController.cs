using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Models;
using TenantManagement.Models.Request;
using TenantManagement.Services;

namespace TenantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userservice;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<UsersController> _logger;
        private readonly IAuthService _authService;

        public UsersController(IUserService userservice, IRequestContext requestContext, ILogger<UsersController> logger,
            IAuthService authService)
        {
            _userservice = userservice;
            _requestContext = requestContext;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Allows a New User to Signup
        /// </summary>
        /// <param name="userData">All required data on the UserModel interface</param>
        /// <param name="tenantId">n/a</param>
        /// <returns>Created User Id</returns>
        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<ActionResult<int>> Signup([Required] UserModel userData, string tenantId = null)
        {
            try
            {
                var user = await _userservice.CreateUser<UserModel>(userData, tenantId);
                return Ok(user.UserId);
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

        /// <summary>
        /// Creates a User
        /// </summary>
        /// <param name="userData">All required data on the UserModel interface</param>
        /// <param name="tenantId">n/a</param>
        /// <param name="template">Email Template String providing context to the request</param>
        /// <param name="verified"></param>
        /// <returns>Created User Id</returns>
        [Authorize]
        [HttpPost()]
        public async Task<ActionResult<int>> CreateUser([Required] UserModel userData, string tenantId = null, TenantManagement.Common.EmailTemplates? template = null, bool verified = false)
        {
            try
            {
                var user = await _userservice.CreateUser<UserModel>(userData, tenantId, template, verified);
                return Ok(user.UserId);
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

        /// <summary>
        /// Reissues Verification Email if User is unverified
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("{id:int}/verification")]
        public async Task<ActionResult<int>> IssueVerification([Required] int id)
        {
            try
            {
                await _userservice.IssueNotification(id, EmailTemplates.Verification);
                return NoContent();
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

        /// <summary>
        /// Reissues Invitation Email to User
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("{id:int}/invitation")]
        public async Task<ActionResult<int>> IssueInvitation([Required] int id)
        {
            try
            {
                await _userservice.IssueNotification(id, EmailTemplates.Invitation);
                return NoContent();
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

        /// <summary>
        /// Gets the User by the User's Id
        /// </summary>
        /// <param name="id">Optional userid - 0 mean to retrieve the current user</param>
        /// <param name="include">Comma separated list of related properties to load i.e. Accounts.Account,Accounts.Roles</param>
        /// <returns>User</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserBaseModel>> GetUser(int id = 0, string include = null)
        {
            try
            {
                var user = await _userservice.GetUser<UserBaseModel>(id <= 0 ? _requestContext.UserId.Value : id, include);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
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
        /// Gets the User by the User's Id
        /// </summary>
        /// <param name="id">Optional userid - 0 mean to retrieve the current user</param>
        /// <param name="include">Comma separated list of related properties to load i.e. Accounts.Account,Accounts.Roles</param>
        /// <returns>User</returns>
        [Authorize]
        [HttpGet("{name}")]
        public async Task<ActionResult<UserBaseModel>> GetUser(string name, string include = null)
        {
            try
            {
                var user = await _userservice.GetUser<UserBaseModel>(name, include);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
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
        /// Gets all the Users based on specified criteria
        /// </summary>
        /// <param name="include">Related objects to load and include with Users</param>
        /// <param name="filter">Filter expression</param>
        /// <param name="sort">List of properties to sort by in order,  '^' indicates descending sort order</param>
        /// <param name="limit">number of records to retrieve</param>
        /// <param name="offset">offset of records to start retrieving</param>
        /// <returns>List of Users</returns>
        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet]
        public async Task<ActionResult<object>> GetUsers(string include = null, string filter = null, [FromQuery] List<string> sort = null, int limit = 0, int offset = 0, bool includeDisabled = false)
        {
            try
            {
                var result = await _userservice.GetUsers<UserModel>(include, filter, sort, limit, offset, null, includeDisabled);
                if (_requestContext.PagingCtx != null)
                {
                    return new PagingModel<UserModel>(_requestContext.PagingCtx, result);
                }

                return Ok(result);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUsers)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet("odata")]
        public async Task<ActionResult<List<UserModel>>> ODataGetList(ODataQueryOptions<User> queryOptions, string include = null, bool includeDisabled = false)
        {
            try
            {
                return Ok(await _userservice.ODataGetList<UserModel>(queryOptions, include, null, includeDisabled));
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ODataGetList)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Edits the User data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userData">All data on the user that is intended to be updated</param>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> EditUser([Required] int id, [Required] UserModel userData)
        {
            try
            {
                await _userservice.EditUser(id, userData);
                return NoContent();
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

        /// <summary>
        /// Deletes a User from the database, only accessable to Admins
        /// </summary>
        /// <param name="id"></param>
        [AuthorizeRoles(Roles.Admin, Roles.AppAdmin)]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser([Required] int id)
        {
            try
            {
                if (!(id > 0))
                {
                    return BadRequest();
                }
                await _userservice.DeleteUser(id);
                return NoContent();
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

        /// <summary>
        /// Sets the Authorized User's Password
        /// </summary>
        /// <param name="request">Object containing the previous password and the new password</param>
        [Authorize]
        [HttpPut("password")]
        public async Task<ActionResult> SetUserPassword([Required] ChangePasswordModel request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.New) && ((!string.IsNullOrEmpty(request.Previous) || _requestContext.Scopes.Contains(TransientScopes.TransientAuthentication.ToString()))))
                {
                    await _userservice.SetPassword((int)_requestContext.UserId, request.New, request.Previous);
                    return NoContent();
                }
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

            return BadRequest();
        }

        /// <summary>
        /// Sets the requested password to the User's Account, accessable only by Admins
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request">Object containing the previous password and the new password</param>
        [AuthorizeRoles(Roles.AppAdmin, Roles.Admin)]
        [HttpPut("{id:int}/password")]
        public async Task<ActionResult> SetPassword([Required] int id, [Required][FromBody] ChangePasswordModel request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.New))
                {
                    await _userservice.SetPassword(id, request.New);
                    return NoContent();
                }
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

            return BadRequest();
        }

        /// <summary>
        /// Resets the User's password and emails a transient token to the associated email
        /// Either Username or UserId is required - not both
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userId"></param>
        [AllowAnonymous]
        [HttpPost("password/reset")]
        public async Task<ActionResult> ResetPassword(string username = null, int userId = 0)
        {
            try
            {
                await _authService.IssueTransientToken(EmailTemplates.PasswordReset, username, userId);
                return NoContent();
            }
            catch (BaseException)
            {
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets the profile image of a User by the User's Id
        /// </summary>
        /// <param name="userId"></param>
        [Authorize]
        [HttpGet("{userId}/profile/image")]
        public async Task<ActionResult> GetImage(int userId)
        {
            try
            {
                var result = await _userservice.GetImage(userId);
                if (result == null)
                {
                    return NotFound();
                }
                return result;
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetImage)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets the profile image of a User by the User's Id
        /// </summary>
        [Authorize]
        [HttpGet("profile/image")]
        public async Task<ActionResult> GetImageById()
        {
            try
            {
                var result = await _userservice.GetImage();
                if (result == null)
                {
                    return NotFound();
                }
                return result;
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetImageById)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Uploads an image to the authorized User's profile
        /// </summary>
        /// <param name="image"></param>
        [Authorize]
        [HttpPost("profile/image")]
        public async Task<ActionResult<int>> UploadImage([Required] IFormFile image)
        {
            try

            {
                await _userservice.UploadImage(image);
                return Ok();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UploadImage)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Deletes the authorized User's image from their profile
        /// </summary>
        [Authorize]
        [HttpDelete("profile/image")]
        public async Task<ActionResult<int>> DeleteImage()
        {
            try
            {
                await _userservice.DeleteImage();
                return Ok();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteImage)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }
    }
}