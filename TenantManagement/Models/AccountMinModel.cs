using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class AccountMinModel : AccountBaseModel
    {
        public List<AccountUserMinModel> Users { get; set; }
    }
}