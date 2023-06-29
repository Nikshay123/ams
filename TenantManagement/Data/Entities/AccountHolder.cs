using System.ComponentModel.DataAnnotations.Schema;
using TenantManagement.Data.Entities.Interfaces;

namespace TenantManagement.Data.Entities
{
    public abstract class AccountHolder : BaseEntity, IAccountHolder
    {
        public int? AccountId { get; set; }

        [NotMapped]
        public Account Account { get; set; }
    }
}