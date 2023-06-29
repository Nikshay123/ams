using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class UserMinModel : UserBaseModel
    {
        public string ContactPhone { get; set; }
        public List<RoleModel> Roles { get; set; } = new();
        public List<AccountUserMinModel> Accounts { get; set; } = new();
    }
}