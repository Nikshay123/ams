using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
using TenantManagement.Models.Response;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IAccountService accountService, IRequestContext requestContext, ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _requestContext = requestContext;
            _logger = logger;
        }

        /// <summary>
        /// Allows a New User to Signup for an Account
        /// </summary>
        /// <param name="model">Object containing an AccountModel, nested Address, and associated Owner. If a UserId exists on Owner, it will find that user and assign them as Owner</param>
        /// <returns>Created Account Id</returns>
        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<ActionResult<AccountCreationResponseModel>> Signup([Required] AccountCreationModel model, string tenantId = null)
        {
            try
            {
                return Ok(await _accountService.Create(model));
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Create)} | {ex.Message} | {System.Text.Json.JsonSerializer.Serialize(model)}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Creates an Account
        /// </summary>
        /// <param name="model">Object containing an AccountModel, nested Address, and associated Owner. If a UserId exists on Owner, it will find that user and assign them as Owner</param>
        /// <returns>Created Account Id</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AccountCreationResponseModel>> Create([Required] AccountCreationModel model, string tenantId = null)
        {
            try
            {
                return Ok(await _accountService.Create(model));
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Create)} | {ex.Message} | {System.Text.Json.JsonSerializer.Serialize(model)}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets the Account by the given Account name
        /// </summary>
        /// <param name="name">Account Name</param>
        /// <param name="include">Related objects to load and include with Account</param>
        /// <returns>Account</returns>
        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet("{name}")]
        public async Task<ActionResult<AccountBaseModel>> GetAccount(string name, string include = null)
        {
            try
            {
                var Account = await _accountService.Get<AccountBaseModel>(null, name, include);
                if (Account == null)
                {
                    return NotFound();
                }

                return Ok(Account);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAccount)} | {ex.Message} | name={name}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets the Account by the given Account Id
        /// </summary>
        /// <param name="id">Account Id</param>
        /// <param name="include">Related objects to load and include with Account</param>
        /// <returns>Account</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountBaseModel>> GetAccount(int id, string include = null)
        {
            try
            {
                var Account = await _accountService.Get<AccountBaseModel>(id, include: include);
                if (Account == null)
                {
                    return NotFound();
                }

                return Ok(Account);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAccount)} | {ex.Message} | id={id}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Gets all the Accounts from the list of Account Ids. Returns all Accounts associated with the User if list is empty
        /// </summary>
        /// <param name="accountIds">List of Account Ids</param>
        /// <param name="include">Related objects to load and include with Accounts</param>
        /// <param name="filter">Filter expression</param>
        /// <param name="sort">List of properties to sort by in order,  '^' indicates descending sort order</param>
        /// <param name="limit">number of records to retrieve</param>
        /// <param name="offset">offset of records to start retrieving</param>
        /// <returns>List of Accounts</returns>
        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet]
        public async Task<ActionResult<object>> GetAccounts([FromQuery] List<int> accountIds = null, string include = null, string filter = null, [FromQuery] List<string> sort = null, int limit = 0, int offset = 0, bool includeDisabled = false)
        {
            try
            {
                var result = await _accountService.GetList<AccountModel>(accountIds, include, filter, sort, limit, offset, null, includeDisabled);
                if (_requestContext.PagingCtx != null)
                {
                    return new PagingModel<AccountModel>(_requestContext.PagingCtx, result);
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
                _logger.LogError(ex, $"{nameof(GetAccounts)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet("odata")]
        public async Task<ActionResult<List<AccountBaseModel>>> ODataGetList(ODataQueryOptions<Account> queryOptions, string include = null, bool includeDisabled = false)
        {
            try
            {
                return Ok(await _accountService.ODataGetList<AccountModel>(queryOptions, include, null, includeDisabled));
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
        /// Gets the GetUserAccounts by the given User Email
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="include">Related objects to load and include with Account</param>
        /// <returns>List of AccountUser</returns>
        [AuthorizeRoles(Roles.AnyUserRole)]
        [HttpGet("{userName:email}")]
        public async Task<ActionResult<AccountUser>> GetUserAccounts(string userName, string include = null)
        {
            try
            {
                var accounts = await _accountService.GetUserAccounts(userName, include);
                if (accounts == null)
                {
                    return NotFound();
                }

                return Ok(accounts);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserAccounts)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Updates specified Account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model">Account Model - only set what should be updated</param>
        /// <returns></returns>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [Required] AccountModel model)
        {
            try
            {
                await _accountService.Update(id, model);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Update)} | {ex.Message} | {System.Text.Json.JsonSerializer.Serialize(model)}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Adds a new User to the specified Account associated with the Account Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user">user model should include username, first/last name, and desired roles</param>
        /// <returns>Created User Id</returns>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpPost("{id}/users")]
        public async Task<ActionResult<int>> AddNewUser([Required] int id, [Required] UserModel user)
        {
            try
            {
                return Ok(await _accountService.AddNewUser(id, user));
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddNewUser)} | {ex.Message} | {id.ToString()} | {System.Text.Json.JsonSerializer.Serialize(user)}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Finds an existing User by username and adds them to the Account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="roles">List of roles to be given to the user</param>
        /// <param name="template">Email template providing context to the request</param>
        /// <returns>nothing</returns>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpPost("{id}/users/{username}")]
        public async Task<ActionResult> AddExistingUser([Required] int id, [Required] string username, List<Roles> roles = null, EmailTemplates? template = null)
        {
            try
            {
                await _accountService.AddExistingUser(id, username, roles, template);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddExistingUser)} | {ex.Message} | {id.ToString()} | {username} | {System.Text.Json.JsonSerializer.Serialize(roles)} | {template?.ToString()}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Finds an existing User by userid and adds them to the Account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="roles">List of roles to be given to the user</param>
        /// <param name="template">Email template providing context to the request</param>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpPost("{id}/users/{userId:int}")]
        public async Task<ActionResult> AddExistingUser([Required] int id, [Required] int userId, List<Roles> roles = null, EmailTemplates? template = null)
        {
            try
            {
                await _accountService.AddExistingUser(id, userId, roles, template);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddExistingUser)} | {ex.Message} | {id.ToString()} | {userId.ToString()} | {System.Text.Json.JsonSerializer.Serialize(roles)} | {template?.ToString()}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Re-sends an email invitation to an existing User on the Account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        [AuthorizeRoles(Roles.AnyAccountManageRole)]
        [HttpPut("{id}/users/{userId:int}/invite")]
        public async Task<ActionResult> IssueAccountInvitation([Required] int id, [Required] int userId)
        {
            try
            {
                await _accountService.IssueAccountInvitation(id, userId);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IssueAccountInvitation)} | {ex.Message} | {id.ToString()} | {userId.ToString()}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Updates Account User Roles
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="roles">Updated Roles List</param>
        /// <returns></returns>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpPut("{id}/users/{userId}")]
        public async Task<ActionResult> UpdateAccountUserRoles([Required] int id, [Required] int userId, [Required][FromBody] List<Roles> roles)
        {
            try
            {
                await _accountService.UpdateAccountUserRoles(id, userId, roles);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateAccountUserRoles)} | {ex.Message} | {id.ToString()} | {userId.ToString()} | {System.Text.Json.JsonSerializer.Serialize(roles)}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Removes a User from the Account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns>Removed User Id</returns>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AnyAccountManageRole)]
        [HttpDelete("{id}/users/{userId}")]
        public async Task<ActionResult> RemoveUser([Required] int id, [Required] int userId)
        {
            try
            {
                await _accountService.RemoveUser(id, userId);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RemoveUser)} | {ex.Message} | {id.ToString()} | {userId.ToString()}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        [AuthorizeRoles(Roles.AnyManageRole, Roles.AccountOwner)]
        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAccount(int accountId)
        {
            try
            {
                await _accountService.DeleteAccount(accountId);
                return NoContent();
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteAccount)} | {ex.Message} | {accountId.ToString()}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }
        }

        /// <summary>
        /// Allows the User to upload an image to an Account where they are an Owner or Admin
        /// </summary>
        /// <param name="image"></param>
        /// <param name="id">The id of the account, if null, will default to the first account where the User is an Owner or Admin</param>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AccountOwner, Roles.AccountAdmin)]
        [HttpPost("profile/image")]
        public async Task<ActionResult<int>> UploadImage([Required] IFormFile image, int? id = null)
        {
            try
            {
                await _accountService.UploadImage(image, id);
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
        /// Gets the profile image of the specified account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A downloadable image</returns>
        [Authorize]
        [HttpGet("{accountId}/profile/image")]
        public async Task<ActionResult> GetImageById(int accountId)
        {
            try
            {
                var result = await _accountService.GetImage(accountId);
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
        /// Gets the profile image of the authorized User's Account
        /// </summary>
        /// <returns>A downloadable image</returns>
        [Authorize]
        [HttpGet("profile/image")]
        public async Task<ActionResult> GetImage()
        {
            try
            {
                var result = await _accountService.GetImage();
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
        /// Allows the User to delete an image to an Account where they are an Owner or Admin
        /// </summary>
        [AuthorizeRoles(Roles.AnyManageRole, Roles.AccountOwner, Roles.AccountAdmin)]
        [HttpDelete("profile/image")]
        public async Task<ActionResult<int>> DeleteImage(int id)
        {
            try
            {
                await _accountService.DeleteImage(id);
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
    }
}