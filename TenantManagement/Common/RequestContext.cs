using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TenantManagement.Common.Interfaces;
using TenantManagement.Common.Permissions;
using TenantManagement.Data.Entities;

namespace TenantManagement.Common
{
    public class RequestContext : IRequestContext
    {
        private const string ENVIRONMENT_PRODUCTION_VALUE = "production";
        private const string ENVIRONMENT_VARIABLE_NAME = "ASPNETCORE_ENVIRONMENT";
        private const string SERVICE_USER_VALUE = "BackgroundService";
        private const string DEFAULT_TENANT_NAME = "DefaultTenant";
        private readonly IConfiguration _config;

        public RequestContext(IConfiguration config, IHttpContextAccessor httpCtx)
        {
            _config = config;

            if (httpCtx != null && httpCtx.HttpContext != null)
            {
                if (httpCtx.HttpContext.Request.Headers.ContainsKey(HeaderNames.Authorization) && httpCtx.HttpContext.Request.Headers[HeaderNames.Authorization].First().StartsWith("Basic "))
                {
                    Authorization = httpCtx.HttpContext.Request.Headers[HeaderNames.Authorization].First().Split(' ').Last();
                }
                else
                {
                    ClaimsUser = httpCtx.HttpContext.User;
                }

                httpCtx.HttpContext.Request.EnableBuffering();
                HttpCtx = httpCtx.HttpContext;
            }
        }

        public ClaimsPrincipal ClaimsUser
        {
            set
            {
                if (value == null) return;

                var username = value.FindFirst(ClaimTypes.Email);
                if (username != null)
                {
                    Username = username.Value;
                }

                var userId = value.FindFirst(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    UserId = int.Parse(userId.Value);
                }

                var tenant = value.Claims.FirstOrDefault(c => c.Type == AppGlobals.ClaimTypeTenantIdUri);

                if (tenant != null)
                {
                    TenantId = Guid.Parse(tenant.Value);
                }
                else
                {
                    Guid tid = (Guid)_config.GetValue(typeof(Guid), DEFAULT_TENANT_NAME);
                    TenantId = tid == null ? null : (Guid)tid;
                }

                var orgs = value.Claims.Where(c => c.Type == AppGlobals.ClaimTypeOrgId);
                if (orgs != null)
                {
                    Orgs = orgs.Select(o => o.Value).ToList();
                }

                var setRoles = true;
                var scopes = value.Claims.Where(c => c.Type == AppGlobals.ClaimTypeScopeId);
                if (scopes != null)
                {
                    Scopes = scopes.Select(s => s.Value).ToList();

                    var currentEnv = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_NAME)?.ToLower();
                    setRoles = currentEnv != ENVIRONMENT_PRODUCTION_VALUE || !Scopes.Contains(AppGlobals.ClaimTypeTestScope);
                }

                if (setRoles)
                {
                    var roles = value.Claims.Where(c => c.Type == ClaimTypes.Role);
                    if (roles != null)
                    {
                        Roles = roles.Select(c => c.Value).ToList();
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException($"Test Account Not Allowed!!! | username: {username}");
                }
            }
        }

        public string Username { get; set; }
        public User User { get; set; }
        public int? UserId { get; private set; }
        public Guid? TenantId { get; set; }
        public IList<string> Orgs { get; private set; }
        public IList<string> Roles { get; private set; }
        public IList<string> Scopes { get; private set; }
        public string ModuleContext { get; set; }
        public string TimezoneInfo { get; set; }
        public string Authorization { get; set; }
        public PagingContext PagingCtx { get; set; }
        public HttpContext HttpCtx { get; private set; }

        public void SetBackgroundContext(Guid tenantId)
        {
            TenantId = tenantId;
            UserId = 0;
            Username = SERVICE_USER_VALUE;
            Roles = new List<string>() { SERVICE_USER_VALUE };
        }

        private PermissionContext pc = null;

        public PermissionContext PermCtx
        {
            get
            {
                if (pc == null)
                {
                    pc = new PermissionContext(this, User);
                }

                return pc;
            }
        }
    }
}