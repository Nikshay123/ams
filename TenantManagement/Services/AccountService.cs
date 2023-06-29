using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using TenantManagement.Common;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManagement.Models;
using TenantManagement.Models.Request;
using TenantManagement.Models.Response;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IAccountRepository _accountRepo;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly IRoleService _roleService;
        private readonly IAccountUserRepository _accountUserRepo;
        private readonly IEmailService _emailService;
        private readonly IRequestContext _reqContext;
        private readonly IProfileStorageService _profileStorageService;
        private readonly IAccountAttributeProvider _attributeProvider;
        private readonly IAddressService _addressService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IConfiguration config, IMapper mapper, IAccountRepository accountrepo, IAuthService authservice, IUserService userservice, IUserRepository userrepo,
            IRoleService roleservice, IAccountUserRepository accountuserrep, IProfileStorageService profileStorageService, IAccountAttributeProvider attributeProvider,
            IEmailService emailService, IAddressService addressService, IRequestContext reqcontext, ILogger<AccountService> logger)
        {
            _config = config;
            _mapper = mapper;
            _accountRepo = accountrepo;
            _authService = authservice;
            _userService = userservice;
            _userRepo = userrepo;
            _roleService = roleservice;
            _accountUserRepo = accountuserrep;
            _emailService = emailService;
            _reqContext = reqcontext;
            _profileStorageService = profileStorageService;
            _attributeProvider = attributeProvider;
            _addressService = addressService;
            _reqContext.ModuleContext = nameof(Account);
            _logger = logger;
        }

        public async Task<T> Get<T>(int? accountId = null, string name = null, string include = null)
        {
            var userInclude = $"{nameof(Account.Users)}.{nameof(AccountUser.User)},{include}";
            Account account;
            if (accountId != null)
            {
                account = await _accountRepo.GetByIdAsync(accountId.Value, userInclude);
            }
            else
            {
                account = await _accountRepo.GetByNameAsync(name, userInclude);

                _reqContext.PermCtx.Account = account;
                //users should only see their associated accounts when using email vs id
                if (account != null && _reqContext.PermCtx.RequestUserInAccount == null && _reqContext.PermCtx.IsAccountContext)
                {
                    account = null;
                }
            }

            if (account != null && !string.IsNullOrEmpty(include))
            {
                await _attributeProvider.GetAttributes(_reqContext, account, include);
            }

            if (typeof(T).Name == nameof(AccountBaseModel))
            {
                _reqContext.PermCtx.Account = account;

                object mapped = null;
                if (!_reqContext.PermCtx.IsAccountContext)
                {
                    mapped = _mapper.MapIgnoreCycles<Account, AccountModel>(account);
                }
                else if (_reqContext.PermCtx.RequestUserInAccount != null && _reqContext.PermCtx.IsAccountManageRole)
                {
                    mapped = _mapper.MapIgnoreCycles<Account, AccountOwnerModel>(account);
                }
                else
                {
                    mapped = _mapper.MapIgnoreCycles<Account, AccountMinModel>(account);
                }
                return (T)mapped;
            }

            return _mapper.Map<Account, T>(account);
        }

        public async Task<List<T>> GetList<T>(List<int> accountIds = null, string include = null, string filter = null, List<string> sort = null, int limit = 0, int offset = 0, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false)
        {
            var accounts = await _accountRepo.GetListAsync(accountIds, $"{nameof(Account.Users)},{include}", filter, sort, limit, offset, predicate, includeDisabled);
            //user should only see their associated accounts
            if (accounts != null && _reqContext.PermCtx.IsAccountContext)
            {
                accounts = accounts.FindAll(x => x.Users.FirstOrDefault(x => x.UserId == _reqContext.UserId) != null).ToList();
            }

            if (typeof(T).Name == nameof(AccountBaseModel))
            {
                object mapped = null;
                if (!_reqContext.PermCtx.IsAccountContext)
                {
                    mapped = _mapper.MapIgnoreCycles<List<Account>, List<AccountModel>>(accounts);
                }
                else if (_reqContext.PermCtx.RequestUserInAccount != null && _reqContext.PermCtx.IsAccountManageRole)
                {
                    mapped = _mapper.MapIgnoreCycles<List<Account>, List<AccountOwnerModel>>(accounts);
                }
                else
                {
                    mapped = _mapper.MapIgnoreCycles<List<Account>, List<AccountMinModel>>(accounts);
                }
                return (List<T>)mapped;
            }

            return _mapper.Map<List<Account>, List<T>>(accounts);
        }

        public virtual async Task<List<TModel>> ODataGetList<TModel>(ODataQueryOptions<Account> queryOptions, string include = null, System.Linq.Expressions.Expression<Func<Account, bool>> predicate = null, bool includeDisabled = false)
        {
            return _mapper.Map<List<Account>, List<TModel>>(await _accountRepo.ODataGetList(queryOptions, include, predicate, includeDisabled));
        }

        public async Task<List<AccountUser>> GetUserAccounts(string userName, string include = null)
        {
            var user = await _userService.GetUser<User>(userName);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }
            var accounts = await _accountUserRepo.GetByAsync(null, user.UserId, include);
            return accounts;
        }

        public async Task<AccountCreationResponseModel> Create(AccountCreationModel model, string tenantId = null)
        {
            if (model.Owner == null)
            {
                if (_reqContext.UserId != null)
                {
                    model.Owner = new UserModel { UserId = _reqContext.UserId.Value };
                }
                else
                {
                    throw new BaseException(HttpStatusCode.BadRequest, "No Account Owner Specified", null, _logger);
                }
            }

            int modelUserId = model.Owner.UserId;

            if (modelUserId > 0 && modelUserId != _reqContext.UserId && !_reqContext.PermCtx.IsManageRole)
            {
                throw new BaseException(HttpStatusCode.Forbidden, "Cannot create account for another user", null, _logger);
            }

            UserModel owner;
            Account account = null;
            User user = null;

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (model.Owner.UserId > 0)
                {
                    owner = await _userService.GetUser<UserModel>(model.Owner.UserId);

                    if (owner == null)
                    {
                        throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
                    }

                    if (owner.Enabled == false)
                    {
                        owner.Enabled = true;
                        await _userService.EditUser(owner.UserId, owner);
                    }

                    user = new()
                    {
                        UserId = owner.UserId
                    };
                }
                else
                {
                    user = await _userService.CreateUser<User>(model.Owner, template: EmailTemplates.None);
                    owner = model.Owner;
                }

                model.Owner = owner;
                account = _mapper.Map<Account>(model);

                if (string.IsNullOrEmpty(tenantId))
                {
                    if (_reqContext.TenantId == null || _reqContext.TenantId == Guid.Empty)
                    {
                        throw new BaseException(System.Net.HttpStatusCode.Conflict, "Invalid Tenant", null, _logger);
                    }

                    account.TenantId = _reqContext.TenantId.Value;
                }
                else if (_reqContext.PermCtx.IsAppAdmin)
                {
                    account.TenantId = Guid.Parse(tenantId);
                }
                else
                {
                    throw new BaseException(System.Net.HttpStatusCode.Unauthorized, "Invalid Tenant", null, _logger);
                }

                try
                {
                    await _accountRepo.Add(account);
                }
                catch (DbUpdateException ex)
                {
                    throw new BaseException(HttpStatusCode.Conflict, $"{nameof(Account)}: {ex.Message}", ex, _logger);
                }

                var accountUser = new AccountUser()
                {
                    UserPrimary = true,
                    Roles = await _roleService.GetRoles<Role>(new List<Roles>() { Roles.AccountOwner }),
                    AccountId = account.AccountId,
                    UserId = user.UserId
                };

                await _accountUserRepo.Add(accountUser);

                scope.Complete();
            }

            if (model.Owner?.UserId <= 0)
            {
                await _authService.IssueNotification(user, EmailTemplates.Verification);
            }

            return new AccountCreationResponseModel { AccountId = account.AccountId, UserId = user.UserId };
        }

        public async Task<int> AddNewUser(int accountId, UserModel userModel)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found", null, _logger);
            }

            User from = null;
            User user = null;

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                var roles = userModel.Roles.FindAll(r => r.Name.StartsWith($"{nameof(Account)}")).Select(x => (Roles)Enum.Parse(typeof(Roles), x.Name)).ToList();
                userModel.Roles = null;
                user = await _userService.CreateUser<User>(userModel, tenantId: account.TenantId.ToString(), template: EmailTemplates.Invitation);
                if (_reqContext.UserId.HasValue)
                {
                    from = await _userService.GetUser<User>(_reqContext.UserId.Value);
                }

                await AddExistingUser(account.AccountId, user.UserId, roles, null);

                scope.Complete();
            }

            if (_emailService.Emails.Count > 0)
            {
                var invitation = _emailService.Emails.FirstOrDefault();
                if (invitation != null)
                {
                    invitation.AddAccount(account);
                }

                if (from != null)
                {
                    invitation.AddFrom(from);
                }

                await _emailService.DispatchEmail();
            }

            return user.UserId;
        }

        public async Task AddExistingUser(int accountId, string username, List<Roles> roles = null, EmailTemplates? template = null)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found", null, _logger);
            }

            var user = await _userService.GetUser<User>(username);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            await AddExistingUser(account, user.UserId, roles, template);
        }

        public async Task AddExistingUser(int accountId, int userId, List<Roles> roles = null, EmailTemplates? template = null)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found", null, _logger);
            }

            await AddExistingUser(account, userId, roles, template);
        }

        public async Task AddExistingUser(Account account, int userId, List<Roles> roles = null, EmailTemplates? template = null)
        {
            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            var user = await _userService.GetUser<User>(userId);
            if (user == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found", null, _logger);
            }

            if (account.TenantId != user.TenantId)
            {
                throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Invalid Tenant Match", null, _logger);
            }

            if ((await _accountUserRepo.GetByAsync(account.AccountId, userId)).Count() > 0)
            {
                throw new BaseException(System.Net.HttpStatusCode.Conflict, "Action Not Allowed. Account User Already Exists!", null, _logger);
            }

            if (account.Users.Count == 0)
            {
                roles = new List<Roles>() { Roles.AccountOwner };
            }
            else if (roles == null)
            {
                roles = new List<Roles>() { Roles.AccountStakeholder };
            }
            else
            {
                roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(roles), true).Select(r => r.Name).ToList();
            }

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                user.Enabled = true;
                await _userRepo.Update(user);

                var accountUser = new AccountUser()
                {
                    Roles = await _roleService.GetRoles<Role>(roles),
                    AccountId = account.AccountId,
                    UserId = userId
                };

                await _accountUserRepo.Add(accountUser);
                scope.Complete();
            }

            if (template != null)
            {
                User from = null;
                if (_reqContext.UserId.HasValue)
                {
                    from = await _userService.GetUser<User>(_reqContext.UserId.Value);
                }

                var emailspec = _emailService.QueueEmailSpec(template.ToString(), from: from, to: user, account: account);
                await _emailService.DispatchEmail();
            }
        }

        public async Task UpdateAccountUserRoles(int accountId, int userId, List<Roles> roles)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found.");
            }

            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            var au = account.Users.FirstOrDefault(x => x.UserId == userId);
            if (au == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account User Not Found", null, _logger);
            }

            if (roles != null)
            {
                au.Roles = _reqContext.PermCtx.ValidateRoleAssignment(await _roleService.GetRoles<Role>(roles), true);
            }

            await _accountUserRepo.Update(au);
        }

        public async Task Update(int accountId, AccountModel model)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)},{nameof(User.Addresses)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound);
            }

            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            account.Name = model.Name ?? account.Name;
            account.ContactEmail = model.ContactEmail ?? account.ContactEmail;
            account.ContactFirstName = model.ContactFirstName ?? account.ContactFirstName;
            account.ContactLastName = model.ContactLastName ?? account.ContactLastName;
            account.ContactPhone = model.ContactPhone ?? account.ContactPhone;
            account.TimezoneInfo = model.TimezoneInfo ?? account.TimezoneInfo;

            var oldAddresses = account.Addresses;
            if (model.Addresses != null)
            {
                if (model.Addresses.Count <= 0)
                {
                    account.Addresses = null;
                }
                else
                {
                    var newAddresses = _mapper.Map<List<AddressModel>, List<Address>>(model.Addresses);
                    newAddresses.ForEach(x => x.AddressId = 0);
                    account.Addresses = newAddresses;
                }
            }

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountRepo.Update(account);
                await _addressService.DeleteRange(oldAddresses);
                scope.Complete();
            }
        }

        public async Task RemoveUser(int accountId, int? userId = null, string username = null)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(Roles)},{nameof(Account.Users)}.{nameof(User)}.{nameof(User.Accounts)}.{nameof(Account)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found.", null, _logger);
            }

            var accountUser = account.Users.FirstOrDefault(x => x.UserId == userId || x.User.Username == username);
            if (accountUser == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "User Not Found.", null, _logger);
            }

            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountUserRepo.Delete(accountUser);
                if (account.Users.Count < 1)
                {
                    account.Enabled = false;
                    await _accountRepo.Update(account);
                }

                if (accountUser.User.Accounts.FirstOrDefault(x => x.Account.Enabled == true) == default && accountUser.User.Roles.Count <= 0)
                {
                    accountUser.User.Enabled = false;
                    var userModel = _mapper.MapIgnoreCycles<User, UserModel>(accountUser.User);
                    await _userService.EditUser(userModel.UserId, userModel);
                }

                scope.Complete();
            }
        }

        public async Task DeleteAccount(int accountId)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(User)},{nameof(Account.Users)}.{nameof(Roles)}");
            _reqContext.PermCtx.Account = account;

            //if in account context and user is not in the account or not accountowner than throw
            if (_reqContext.PermCtx.IsAccountContext &&
                (_reqContext.PermCtx.RequestUserInAccount == null ||
                _reqContext.PermCtx.RequestUserInAccount.Roles?.FirstOrDefault(x => x.Name == Roles.AccountOwner) == null))
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            var accountUsers = new List<AccountUser>(account.Users);
            User owner = null;
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var user in accountUsers)
                {
                    if (user.Roles.FirstOrDefault(x => x.Name == Roles.AccountOwner) != null)
                    {
                        owner = user.User;
                        continue;
                    }

                    await RemoveUser(accountId, user.UserId);
                }

                account.Enabled = false;
                await _accountRepo.Update(account);
                scope.Complete();
            }

            foreach (var au in accountUsers)
            {
                _emailService.QueueEmailSpec(EmailTemplates.AccountDeactivated.ToString(), from: owner, to: au.User, account: account);
            }

            await _emailService.DispatchEmail();
        }

        public async Task UploadImage(IFormFile image, int? id = null)
        {
            if (!int.TryParse(_reqContext.Orgs?.FirstOrDefault(), out int userAccountId) && id == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Id not valid", null, _logger);
            }

            int accountId = id ??= userAccountId;
            Account account = await Get<Account>(accountId);

            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }
            string imageId = await _profileStorageService.UploadImage(image, accountId);
            account.ProfileImageId = imageId;
            await _accountRepo.Update(account);
        }

        public async Task DeleteImage(int? id = null)
        {
            if (!int.TryParse(_reqContext.Orgs?.FirstOrDefault(), out int userAccountId) && id == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Id not valid", null, _logger);
            }

            int accountId = id ??= userAccountId;
            Account account = await Get<Account>(accountId);

            _reqContext.PermCtx.Account = account;
            if (_reqContext.PermCtx.IsAccountContext && _reqContext.PermCtx.RequestUserInAccount == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.Forbidden, "Action Not Allowed", null, _logger);
            }

            await _profileStorageService.DeleteImage(accountId);
            account.ProfileImageId = null;
            await _accountRepo.Update(account);
        }

        public async Task<FileStreamResult> GetImage(int? id = null)
        {
            if (!int.TryParse(_reqContext.Orgs?.FirstOrDefault(), out int userAccountId) && id == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.BadRequest, "Id not valid", null, _logger);
            }

            int accountId = id ??= userAccountId;
            Account account = await Get<Account>(accountId);

            if (string.IsNullOrEmpty(account.ProfileImageId))
            {
                return null;
            }

            return _profileStorageService.GetImage(accountId, account.ProfileImageId);
        }

        public async Task IssueAccountInvitation(int accountId, int userId, EmailTemplates template = EmailTemplates.Invitation)
        {
            var account = await Get<Account>(accountId, include: $"{nameof(Account.Users)}.{nameof(User)}");
            if (account == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Account Not Found", null, _logger);
            }

            var accountUser = account.Users.FirstOrDefault(u => u.UserId == userId);
            if (accountUser == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.BadRequest, "User Not Found", null, _logger);
            }

            if (accountUser.User.LatestLogin == null)
            {
                using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _authService.IssueNotification(accountUser.User, template);
                    scope.Complete();
                }

                if (_emailService.Emails.Count > 0)
                {
                    var invitation = _emailService.Emails.FirstOrDefault();
                    if (invitation != null)
                    {
                        invitation.AddAccount(account);

                        User from = null;
                        if (_reqContext.UserId.HasValue)
                        {
                            from = await _userService.GetUser<User>(_reqContext.UserId.Value);
                        }

                        if (from != null)
                        {
                            invitation.AddFrom(from);
                        }
                    }
                    await _emailService.DispatchEmail();
                }
            }
        }
    }
}