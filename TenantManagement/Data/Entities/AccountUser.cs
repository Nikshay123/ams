using System.Collections.Generic;

namespace TenantManagement.Data.Entities
{
    public class AccountUser : BaseEntity
    {
        public Account Account { get; set; }
        public int AccountId { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        public bool? UserPrimary { get; set; }

        public List<Role> Roles { get; set; } = new();
    }
}