using System;
using System.Collections.Generic;

namespace TenantManagement.Models.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string TenantId { get; set; }
        public IList<string> Roles { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}