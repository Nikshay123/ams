using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace TenantManagement.Models
{
    public class EmailBaseModel : PageModel
    {
        public EmailBaseModel(Dictionary<string, object> data)
        {
            Data = data;
        }

        public Dictionary<string, object> Data { get; set; }
    }
}