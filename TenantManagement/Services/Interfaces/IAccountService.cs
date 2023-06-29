using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Data.Entities;
using TenantManagement.Models;
using TenantManagement.Models.Request;
using TenantManagement.Models.Response;

namespace TenantManagement.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountCreationResponseModel> Create(AccountCreationModel model, string tenantId = null);

        Task<T> Get<T>(int? accountId = null, string name = null, string include = null);

        Task<List<T>> GetList<T>(List<int> accountIds = null, string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false);

        Task<List<AccountUser>> GetUserAccounts(string userName, string include = null);

        Task<int> AddNewUser(int accountId, UserModel user);

        Task AddExistingUser(int accountId, int userId, List<Roles> roles = null, EmailTemplates? template = null);

        Task AddExistingUser(int accountId, string username, List<Roles> roles = null, EmailTemplates? template = null);

        Task RemoveUser(int accountId, int? userId = null, string username = null);

        Task Update(int id, AccountModel model);

        Task UpdateAccountUserRoles(int accountId, int userId, List<Roles> roles);

        Task UploadImage(IFormFile image, int? id = null);

        Task DeleteImage(int? id = null);

        Task<FileStreamResult> GetImage(int? id = null);

        Task IssueAccountInvitation(int accountId, int userId, EmailTemplates template = EmailTemplates.Invitation);

        Task<List<TModel>> ODataGetList<TModel>(ODataQueryOptions<Account> queryOptions, string include = null, Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false);

        Task DeleteAccount(int accountId);
    }
}