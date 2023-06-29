using System;
using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class UserLeafModel : UserBaseModel
    {
        public string Userpass { get; set; }
        public string ContactPhone { get; set; }
        public List<RoleModel> Roles { get; set; }
        public List<ScopeModel> Scopes { get; set; } = new();
        public List<AddressModel> Addresses { get; set; }
        public DateTime? LatestLogin { get; set; }
    }
}