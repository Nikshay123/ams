using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class AccountUserMinModel
    {
        public int AccountId { get; set; }
        public AccountBaseModel Account { get; set; }

        public int UserId { get; set; }
        public UserBaseModel User { get; set; }

        public List<RoleModel> Roles { get; set; }
    }
}