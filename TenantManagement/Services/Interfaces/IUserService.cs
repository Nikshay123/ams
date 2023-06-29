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

namespace TenantManagement.Services
{
    public interface IUserService
    {
        Task SetPassword(int id, string password, string previousPassword = null);

        Task<T> CreateUser<T>(UserModel userData, string tenantId = null, EmailTemplates? template = null, bool verified = false);

        Task<T> GetUser<T>(string username, string include = null);

        Task<T> GetUser<T>(int userId, string include = null);

        Task<List<T>> GetUsers<T>(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, Expression<Func<User, bool>> predicate = null, bool includeDisabled = false);

        Task EditUser(int id, UserModel userData);

        Task DeleteUser(int doomedUserId);

        Task UploadImage(IFormFile image);

        Task DeleteImage();

        Task<FileStreamResult> GetImage(int? id = null);

        Task<List<TModel>> ODataGetList<TModel>(ODataQueryOptions<User> queryOptions, string include = null, Expression<Func<User, bool>> predicate = null, bool includeDisabled = false);

        Task IssueNotification(int userId, EmailTemplates template);
    }
}