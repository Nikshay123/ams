using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantManagement.Data.Entities.Interfaces
{
    public interface IAccountHolder
    {
        int? AccountId { get; set; }
        Account Account { get; set; }
    }
}