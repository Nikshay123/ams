using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using TenantManagement.Common.Permissions;
using TenantManagement.Data.Entities;

namespace TenantManagement.Common.Interfaces
{
    public interface IRequestContext
    {
        ClaimsPrincipal ClaimsUser { set; }
        string Username { get; set; }
        User User { get; }
        int? UserId { get; }
        Guid? TenantId { get; set; }
        IList<string> Roles { get; }
        IList<string> Orgs { get; }
        IList<string> Scopes { get; }
        string ModuleContext { get; set; }
        string Authorization { get; set; }

        public string TimezoneInfo { get; set; }
        PermissionContext PermCtx { get; }
        HttpContext HttpCtx { get; }
        PagingContext PagingCtx { get; set; }

        void SetBackgroundContext(Guid tenantId);
    }
}