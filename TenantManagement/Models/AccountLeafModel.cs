using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class AccountLeafModel : AccountBaseModel
    {
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public List<AddressModel> Addresses { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
    }
}