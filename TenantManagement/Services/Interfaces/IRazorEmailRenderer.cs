using System.Collections.Generic;
using System.Threading.Tasks;

namespace TenantManagement.Services.Interfaces
{
    public interface IRazorEmailRenderer
    {
        Task<string> Render(Dictionary<string, object> data, string template);
    }
}