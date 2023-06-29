using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class AccountUserModel
    {
        public int AccountId { get; set; }
        public AccountLeafModel Account { get; set; }

        public int UserId { get; set; }
        public UserLeafModel User { get; set; }

        public List<RoleModel> Roles { get; set; }
    }
}