using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using TenantManagement.Common;
using TenantManagement.Common.Constants;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManagement.Models;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;
        private readonly IUserRepository _userRepo;
        private readonly IRequestContext _reqContext;
        private readonly IProfileStorageService _profileStorageService;
        private readonly IAddressService _addressService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(IRoleService rolessvc, IUserRepository userrepo, IRequestContext reqcontext, IMapper mapper,
            ILogger<UserService> logger, IAuthService authService, IProfileStorageService profileStorageService,
            IAddressService addressService, IEmailService emailService)
        {
            _roleService = rolessvc;
            _userRepo = userrepo;
            _reqContext = reqcontext;
            _mapper = mapper;
            _logger = logger;
            _authService = authService;
            _reqContext.ModuleContext = nameof(User);
            _profileStorageService = profileStorageService;
            _addressService = addressService;
            _emailService = emailService;
        }

        public async Task IssueNotification(int userId, EmailTemplates template)
        {
            if (template != EmailTemplates.Verification && template != EmailTemplates.Invitation)
            {
                throw new BaseException(HttpStatusCode.BadRequest, "Invalid Notification", null, _logger);
            }

            var user = await GetUser<User>(userId);
            if (user != null && user.Verified != true)
            {
                _reqContext.PermCtx.User = user;
                if (_reqContext.PermCtx.IsAccountContext && !_reqContext.PermCtx.IsSelf)
                {
                    throw new BaseException(HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
                }

                await _authService.IssueNotification(user, template);
            }
        }

        public async Task SetPassword(int id, string password, string previousPassword = null)
        {
            if ((string.IsNullOrEmpty(previousPassword) && !_reqContext.Scopes.Contains(TransientScopes.TransientAuthentication.ToString()) && !_reqContext.PermCtx.IsManageRole) ||
                !ValidatePasswordPattern(password))
            {
                throw new BaseException(HttpStatusCode.BadRequest, "Invalid Password", null, _logger);
            }

            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            _reqContext.PermCtx.User = user;
            if (!_reqContext.PermCtx.SameUserTenant && !_reqContext.PermCtx.IsAppAdmin)
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Invalid Tenant", null, _logger);
            }

            var aka = _reqContext.Scopes.FirstOrDefault(s => s.StartsWith(nameof(TransientScopes.TransientAka)));
            if (aka != null)
            {
                var parts = aka.Split(":");
                if (parts.Length > 1)
                {
                    user.Username = parts[1];
                }
            }

            if (!string.IsNullOrEmpty(previousPassword))
            {
                var legacyHash = CryptoUtils.HashPassword(user.Username, previousPassword, user.Salt, true);
                var oldHash = CryptoUtils.HashPassword(user.Username, previousPassword, user.Salt);
                if (legacyHash != user.Userhash && oldHash != user.Userhash)
                {
                    throw new BaseException(HttpStatusCode.Unauthorized, "Invalid Username/Password", null, _logger);
                }
            }

            var passLegacyHash = CryptoUtils.HashPassword(user.Username.ToLower(), password, user.Salt, true);
            var passOldHash = CryptoUtils.HashPassword(user.Username.ToLower(), password, user.Salt);
            if (passOldHash == user.Userhash || passLegacyHash == user.Userhash)
            {
                throw new BaseException(HttpStatusCode.Conflict, "Can't Reuse Old Password", null, _logger);
            }

            user.TransientAuthToken = null;
            user.TransientAuthExpiry = null;
            user.TransientContext = null;
            user.Salt = Guid.NewGuid().ToString();
            user.Userhash = CryptoUtils.HashPassword(user.Username.ToLower(), password, user.Salt);
            await _userRepo.Update(user);
        }

        public async Task<T> CreateUser<T>(UserModel userData, string tenantId = null, EmailTemplates? template = null, bool verified = false)
        {
            try
            {
                if (_reqContext.UserId >= 0 && !_reqContext.PermCtx.IsManageRole && !_reqContext.PermCtx.IsAccountManageRole)
                {
                    throw new BaseException(HttpStatusCode.Unauthorized, "Action Not Allowed.", null, _logger);
                }

                userData = TrimWhiteSpace(userData);
                ValiateUserData(userData, template != EmailTemplates.Invitation);

                User user = _mapper.Map<User>(userData);

                if (!string.IsNullOrEmpty(tenantId) && _reqContext.TenantId?.ToString() != tenantId)
                {
                    Guid tid;
                    if (!Guid.TryParse(tenantId, out tid))
                    {
                        throw new BaseException(HttpStatusCode.BadRequest, "Invalid Tenant Id", null, _logger);
                    }

                    if (_reqContext.PermCtx.IsAppAdmin)
                    {
                        user.TenantId = tid;
                    }
                    else
                    {
                        throw new BaseException(HttpStatusCode.Forbidden, "Invalid Tenant Id", null, _logger);
                    }
                }
                else
                {
                    user.TenantId = _reqContext.TenantId.Value;
                }

                user.Username = user.Username.ToLower();
                user.Salt = Guid.NewGuid().ToString();
                user.Userhash = CryptoUtils.HashPassword(user.Username, string.IsNullOrEmpty(userData.Userpass) ? DateTime.Now.Ticks.ToString() : userData.Userpass, user.Salt);
                user.Verified = verified;

                if (_reqContext.UserId > 0)
                {
                    if (userData.Roles != null && userData.Roles.Count > 0)
                    {
                        if (userData.Roles.FirstOrDefault(x => string.IsNullOrEmpty(x.Name)) == null)
                        {
                            user.Roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(userData.Roles.Select(r => r.Name).ToList()));
                        }
                        else
                        {
                            user.Roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(userData.Roles.Select(r => r.RoleId).ToList()));
                        }
                    }
                }

                await _userRepo.CreateUser(user);

                if (template != null && template != EmailTemplates.None)
                {
                    await _authService.IssueNotification(user, template.Value);
                }

                return _mapper.Map<User, T>(user);
            }
            catch (DbUpdateException ex)
            {
                throw new BaseException(HttpStatusCode.Conflict, $"{nameof(User)}: {ex.Message}", ex, _logger);
            }
        }

        public async Task<T> GetUser<T>(int userId, string include = null)
        {
            return await GetUserBase<T>(await _userRepo.GetByIdAsync(userId, include));
        }

        public async Task<T> GetUser<T>(string username, string include = null)
        {
            return await GetUserBase<T>(await _userRepo.GetByNameAsync(username, include));
        }

        protected async Task<T> GetUserBase<T>(User user)
        {
            if (user == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }
            _reqContext.PermCtx.User = user;

            if (typeof(T).Name == nameof(UserBaseModel))
            {
                object mapped = null;
                if (!_reqContext.PermCtx.IsAccountContext)
                {
                    mapped = _mapper.MapIgnoreCycles<User, UserModel>(user);
                }
                else if (_reqContext.PermCtx.IsSelf)
                {
                    mapped = _mapper.MapIgnoreCycles<User, UserSelfModel>(user);
                }
                else
                {
                    mapped = _mapper.MapIgnoreCycles<User, UserMinModel>(user);
                }
                return (T)mapped;
            }

            return _mapper.Map<User, T>(user);
        }

        public async Task<List<T>> GetUsers<T>(string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<User, bool>> predicate = null, bool includeDisabled = false)
        {
            return _mapper.MapIgnoreCycles<List<User>, List<T>>(await _userRepo.GetList(include, filter, sort, limit, offset, predicate, includeDisabled));
        }

        public virtual async Task<List<TModel>> ODataGetList<TModel>(ODataQueryOptions<User> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<User, bool>> predicate = null, bool includeDisabled = false)
        {
            return _mapper.Map<List<User>, List<TModel>>(await _userRepo.ODataGetList(queryOptions, include, predicate, includeDisabled));
        }

        public async Task EditUser(int id, UserModel userData)
        {
            User user = await _userRepo.GetByIdAsync(id, $"{nameof(User.Roles)},{nameof(User.Addresses)},{nameof(User.Accounts)}");
            if (user == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            _reqContext.PermCtx.User = user;
            if (!_reqContext.PermCtx.IsSelf && !(_reqContext.PermCtx.IsManageRole || _reqContext.PermCtx.IsAccountManageRole))
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            if (userData.Roles != null)
            {
                user.Roles.Clear();
                if (userData.Roles.Count > 0)
                {
                    if (userData.Roles.FirstOrDefault(x => string.IsNullOrEmpty(x.Name)) == null)
                    {
                        user.Roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(userData.Roles.Select(r => r.Name).ToList()));
                    }
                    else
                    {
                        user.Roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(userData.Roles.Select(r => r.RoleId).ToList()));
                    }
                }
            }

            var oldAddresses = user.Addresses;
            if (userData.Addresses != null)
            {
                if (userData.Addresses.Count <= 0)
                {
                    user.Addresses = null;
                }
                else
                {
                    var newAddresses = _mapper.Map<List<AddressModel>, List<Address>>(userData.Addresses);
                    newAddresses.ForEach(x => x.AddressId = 0);
                    user.Addresses = newAddresses;
                }
            }

            if (!string.IsNullOrEmpty(userData.Username) && user.Username.ToLower() != userData.Username.ToLower())
            {
                _reqContext.PermCtx.User = user;
                if (!_reqContext.PermCtx.IsSelf)
                {
                    throw new BaseException(HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
                }

                var matchingUser = await _userRepo.GetByNameAsync(userData.Username.ToLower());
                if (matchingUser != null)
                {
                    throw new BaseException(HttpStatusCode.Conflict, "Action Not Allowed! Email is already in use.", null, _logger);
                }

                user.TransientContext = $"{nameof(TransientScopes.TransientAka)}:{userData.Username.ToLower()}";
            }
            else
            {
                user.TransientContext = null;
            }

            user.FirstName = userData.FirstName ?? user.FirstName;
            user.LastName = userData.LastName ?? user.LastName;
            user.ContactPhone = userData.ContactPhone ?? user.ContactPhone;
            user.Verified = userData.Verified ?? user.Verified;
            user.TimezoneInfo = userData.TimezoneInfo ?? user.TimezoneInfo;
            user.Enabled = (userData.Enabled.HasValue && userData.Enabled.Value) || user.Roles.Count != 0 || user.Accounts.Count != 0;

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _userRepo.Update(user);
                await _addressService.DeleteRange(oldAddresses);

                if (!string.IsNullOrEmpty(user.TransientContext) && user.TransientContext.StartsWith(nameof(TransientScopes.TransientAka)))
                {
                    var emailSpec = _emailService.QueueEmailSpec(nameof(EmailTemplates.ChangeEmailNotification), to: user);
                    emailSpec.ModelData[nameof(EmailDataKeys.Id)] = userData.Username;

                    await _authService.IssueNotification(user, EmailTemplates.ChangeEmailPasswordReset);
                    emailSpec = _emailService.Emails.Last();
                    emailSpec.Recipients = userData.Username;
                }

                scope.Complete();
            }

            await _emailService.DispatchEmail();
        }

        public async Task DeleteUser(int userId)
        {
            Guid tenantId = _reqContext.TenantId.Value;
            User user = await _userRepo.GetByIdAsync(userId, Constants.IncludeAll);

            if (user == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            _reqContext.PermCtx.User = user;
            if (!_reqContext.PermCtx.IsAppAdmin && !_reqContext.PermCtx.IsAdmin)
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Cannot delete user from another tenant", null, _logger);
            }

            user.Roles = new();
            user.Accounts = new();
            user.Scopes = new();
            user.Enabled = false;
            await _userRepo.Update(user);
        }

        public async Task<FileStreamResult> GetImage(int? id = null)
        {
            int realId = id ??= _reqContext.UserId.Value;
            User user = await _userRepo.GetByIdAsync(realId);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            if (string.IsNullOrEmpty(user.ProfileImageId))
            {
                return null;
            }

            return _profileStorageService.GetImage(realId, user.ProfileImageId);
        }

        public async Task UploadImage(IFormFile image)
        {
            User user = await _userRepo.GetByIdAsync((int)_reqContext.UserId);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            _reqContext.PermCtx.User = user;
            if (!_reqContext.PermCtx.IsSelf)
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Action Not Allowed.", null, _logger);
            }

            string imageId = await _profileStorageService.UploadImage(image, user.UserId);
            user.ProfileImageId = imageId;
            await _userRepo.Update(user);
        }

        public async Task DeleteImage()
        {
            User user = await _userRepo.GetByIdAsync((int)_reqContext.UserId);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            _reqContext.PermCtx.User = user;
            if (!_reqContext.PermCtx.IsSelf && !_reqContext.PermCtx.IsManageRole)
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Action Not Allowed.", null, _logger);
            }

            await _profileStorageService.DeleteImage(user.UserId);
            user.ProfileImageId = null;
            await _userRepo.Update(user);
        }

        #region Protected Methods

        protected bool ValiateUserData(UserModel userData, bool throwException = false)
        {
            bool isValidEmailPattern = ValidateEmailPattern(userData.Username);
            bool isPasswordStrong = ValidatePasswordPattern(userData.Userpass);

            if (throwException && (!isValidEmailPattern || !isPasswordStrong))
            {
                throw new BaseException(HttpStatusCode.BadRequest, $"Email or Password data is invalid | Email Requirement: {(isValidEmailPattern ? "pass" : "fail")} | Password Requirement: {(isPasswordStrong ? "pass" : "fail")}", null, _logger);
            }
            return isValidEmailPattern && isPasswordStrong;
        }

        private static bool ValidateEmailPattern(string email)
        {
            if (email.EndsWith("."))
            {
                return false;
            }
            try
            {
                MailAddress addr = new(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidatePasswordPattern(string password)
        {
            string hasUpperCase = @"[A-Z]";
            string hasLowerCase = @"[a-z]";
            string hasNumeric = @"[0-9]";
            string hasSymbol = @"[@$!#%*?&]";

            if (password == null || password.Length < Constants.MinPasswordLength)
            {
                return false;
            }

            List<string> regexList = new() { hasUpperCase, hasLowerCase, hasNumeric, hasSymbol };
            foreach (string regex in regexList)
            {
                Regex rgx = new(regex);
                if (!rgx.Match(password).Success)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<List<Role>> AssignRoles(List<RoleModel> roles)
        {
            List<int> roleIds = new();

            foreach (RoleModel role in roles)
            {
                roleIds.Add(role.RoleId);
            }

            return await _roleService.GetRoles<Role>(roleIds);
        }

        private static UserModel TrimWhiteSpace(UserModel userData)
        {
            userData.Username = userData.Username?.Trim();
            userData.FirstName = userData.FirstName?.Trim();
            userData.LastName = userData.LastName?.Trim();
            userData.ContactPhone = userData.ContactPhone?.Trim();
            return userData;
        }

        #endregion Protected Methods
    }
}