using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TenantManagement.Common;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Models.Response;
using TenantManagement.Services;

namespace TenantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IRequestContext requestContext, ILogger<AuthController> logger, IAuthService authService)
        {
            _requestContext = requestContext;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Gets User Access Token from Basic Authentication
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<LoginResponse>> AuthToken()
        {
            string username = null;
            try
            {
                if (!string.IsNullOrEmpty(_requestContext.Authorization))
                {
                    byte[] data = Convert.FromBase64String(_requestContext.Authorization);
                    var basicAuthStr = Encoding.UTF8.GetString(data);
                    var creds = basicAuthStr.Split(':');

                    if (creds.Length > 1)
                    {
                        username = creds[0];
                        var resp = await _authService.Authenticate(username, creds[1]);
                        if (resp != null)
                        {
                            return resp;
                        }
                    }
                }
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AuthToken)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }

            return Unauthorized();
        }

        /// <summary>
        /// Gets Access Token for specified User
        /// </summary>
        [AuthorizeRoles(Roles.AppAdmin, Roles.Admin)]
        [HttpGet("user")]
        public async Task<ActionResult<LoginResponse>> UserToken([FromQuery] int? id = null, [FromQuery] string name = null)
        {
            try
            {
                if (id.HasValue || !string.IsNullOrEmpty(name))
                {
                    _logger.LogInformation($"User Token Request for | id: {id} | name: {name}");
                    return await _authService.GetAuthToken(id, name);
                }
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UserToken)} | {ex.Message} | id={id} | name={name}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }

            return Unauthorized();
        }

        /// <summary>
        /// Gets Access Token for User and Account
        /// </summary>
        [Authorize]
        [HttpGet("account")]
        public async Task<ActionResult<LoginResponse>> UserAccount(int? accountId)
        {
            try
            {
                _logger.LogInformation($"Account Token Request for | id: {accountId}");
                return await _authService.GetAccountAuthToken(accountId);
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UserAccount)} | {ex.Message} | id={accountId}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }

            return Unauthorized();
        }

        /// <summary>
        /// Gets Access Token from Refresh Token specified in Basic Auth Credentials i.e. Username:RefreshToken
        /// </summary>
        [AllowAnonymous]
        [HttpGet("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken()
        {
            string username = null;
            try
            {
                if (!string.IsNullOrEmpty(_requestContext.Authorization))
                {
                    byte[] data = Convert.FromBase64String(_requestContext.Authorization);
                    var basicAuthStr = Encoding.UTF8.GetString(data);
                    var creds = basicAuthStr.Split(':');

                    if (creds.Length > 1)
                    {
                        username = creds[0];
                        var resp = await _authService.RefreshAuthToken(username, creds[1]);
                        if (resp != null)
                        {
                            return Ok(resp);
                        }
                    }
                }
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RefreshToken)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }

            return Unauthorized();
        }

        /// <summary>
        /// Gets Access Token from Password Reset Code specified in Basic Auth Credentials i.e. Username:ResetCode
        /// </summary>
        [AllowAnonymous]
        [HttpGet("transient")]
        public async Task<ActionResult<LoginResponse>> ResetToken()
        {
            string username = null;
            try
            {
                if (!string.IsNullOrEmpty(_requestContext.Authorization))
                {
                    byte[] data = Convert.FromBase64String(_requestContext.Authorization);
                    var basicAuthStr = Encoding.UTF8.GetString(data);
                    var creds = basicAuthStr.Split(':');

                    if (creds.Length > 1)
                    {
                        username = creds[0];
                        var resp = await _authService.TransientAuthToken(username, creds[1]);
                        if (resp != null)
                        {
                            return Ok(resp);
                        }
                    }
                }
            }
            catch (BaseException ex)
            {
                return Problem(statusCode: (int)ex.ErrCode, title: ex.Message,
                    type: ex.ErrCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ResetToken)} | {ex.Message}");
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, title: ex.Message,
                    type: ((int)HttpStatusCode.BadRequest).ToString());
            }

            return Unauthorized();
        }
    }
}