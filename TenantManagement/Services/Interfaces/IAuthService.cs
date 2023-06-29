using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Data.Entities;
using TenantManagement.Models.Response;

namespace TenantManagement.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Authenticate(string username, string password);

        Task<LoginResponse> GetAuthToken(int? userid = null, string username = null);

        Task<LoginResponse> RefreshAuthToken(string username, string refreshToken);

        Task<LoginResponse> TransientAuthToken(string username, string refreshToken);

        Task IssueTransientToken(EmailTemplates template, string username = null, int userid = 0);

        Task<LoginResponse> GetAccountAuthToken(int? accountId = null);

        Task IssueNotification(User user, EmailTemplates template);
    }
}