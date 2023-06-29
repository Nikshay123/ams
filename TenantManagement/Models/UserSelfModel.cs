using System;
using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class UserSelfModel : UserBaseModel
    {
        public string Userpass { get; set; }
        public string ContactPhone { get; set; }
        public List<RoleModel> Roles { get; set; } = new();
        public List<AccountUserMinModel> Accounts { get; set; } = new();
        public List<ScopeModel> Scopes { get; set; } = new();
        public List<AddressModel> Addresses { get; set; }
        public DateTime? LatestLogin { get; set; }
    }
}