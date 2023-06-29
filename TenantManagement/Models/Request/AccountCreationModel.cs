using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantManagement.Models.Request
{
    public class AccountCreationModel : AccountModel
    {
        public UserModel Owner { get; set; }
    }
}