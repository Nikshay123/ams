using System;
using System.Collections.Generic;
using System.Linq;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TMRoles = TenantManagement.Common.Roles;

namespace TenantManagement.Common.Permissions
{
    public class PermissionContext
    {
        public IRequestContext ReqCtx { get; private set; }

        private User _user;

        public User User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    LoadContext();
                }
            }
        }

        private Account _account;

        public Account Account
        {
            get { return _account; }
            set
            {
                if (_account != value)
                {
                    _account = value;
                    LoadContext();
                }
            }
        }

        public bool SameUserTenant { get; private set; }
        public bool SameAccountTenant { get; private set; }
        public bool SameUserAccountTenant { get; private set; }
        public bool IsAnonymous { get; private set; }
        public bool IsSelf { get; private set; }
        public bool IsAppAdmin { get; private set; }
        public bool IsAccountContext { get; private set; }
        public bool IsNonAccountRole => IsAdmin || IsManager || IsUser;
        public AccountUser RequestUserInAccount { get; private set; }
        public AccountUser UserInAccount { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsManager { get; private set; }
        public bool IsUser { get; private set; }
        public bool IsManageRole => IsAdmin || IsManager;
        public bool IsAccountOwner { get; private set; }
        public bool IsAccountAdmin { get; private set; }
        public bool IsAccountManager { get; private set; }
        public bool IsAccountUser { get; private set; }
        public bool IsAccountStakeholder { get; private set; }
        public bool IsAccountManageRole => IsAccountOwner || IsAccountAdmin || IsAccountManager;
        public List<Roles> Roles { get; private set; }

        public PermissionContext(IRequestContext requestContext, User userContext = null, Account accountContext = null)
        {
            ReqCtx = requestContext;
            this._user = userContext;
            this._account = accountContext;
            LoadContext();
        }

        public List<Role> ValidateRoleAssignment(List<Role> roles, bool accountRoles = false)
        {
            if (!ReqCtx.PermCtx.IsManageRole && !ReqCtx.PermCtx.IsAccountManageRole)
            {
                throw new BaseException(System.Net.HttpStatusCode.Unauthorized, "Action Not Allowed.");
            }

            var maxUserRole = ReqCtx.Roles.Select(r => (int)Enum.Parse(typeof(Roles), r)).Min();
            var maxAccountUserRole = roles.Select(r => (int)r.Name).Min();
            if (maxAccountUserRole < maxUserRole)
            {
                //user can't give role greater than their role
                throw new BaseException(System.Net.HttpStatusCode.Unauthorized, "Action Not Allowed.  Invalid Role(s)");
            }

            if (accountRoles)
            {
                return roles.FindAll(r => r.Name.ToString().StartsWith($"{nameof(Account)}")).ToList();
            }

            return roles;
        }

        protected void LoadContext()
        {
            IsAnonymous = ReqCtx.TenantId == null || ReqCtx.UserId <= 0;
            IsSelf = ReqCtx.UserId == User?.UserId;
            IsAppAdmin = ReqCtx.TenantId == Guid.Empty;
            IsAccountContext = !(ReqCtx.Orgs == null || ReqCtx.Orgs.Count <= 0);
            SameUserTenant = ReqCtx.TenantId == User?.TenantId;
            SameAccountTenant = ReqCtx.TenantId == Account?.TenantId;
            SameUserAccountTenant = User?.TenantId == Account?.TenantId;

            if (IsAccountContext && Account != null && Account.Users != null)
            {
                RequestUserInAccount = Account.Users.FirstOrDefault(u => u.UserId == ReqCtx.UserId);
                if (User != null)
                {
                    UserInAccount = Account.Users.FirstOrDefault(u => u.UserId == User.UserId);
                }
            }

            foreach (string role in ReqCtx.Roles)
            {
                if (IsAccountContext)
                {
                    IsAccountAdmin = IsAccountAdmin || role == TMRoles.AccountAdmin.ToString();
                    IsAccountOwner = IsAccountOwner || role == TMRoles.AccountOwner.ToString();
                    IsAccountManager = IsAccountManager || role == TMRoles.AccountManager.ToString();
                    IsAccountUser = IsAccountUser || role == TMRoles.AccountUser.ToString();
                    IsAccountStakeholder = IsAccountStakeholder || role == TMRoles.AccountStakeholder.ToString();
                }
                else
                {
                    IsAdmin = IsAdmin || role == TMRoles.Admin.ToString();
                    IsManager = IsManager || role == TMRoles.Manager.ToString();
                    IsUser = IsUser || role == TMRoles.User.ToString();
                }
            }

            Roles = ReqCtx.Roles.Select(r =>
            {
                object? role;
                return Enum.TryParse(typeof(Roles), r, true, out role) ? (Roles)role : TMRoles.None;
            }).ToList().FindAll(r => r != TMRoles.None).ToList();
        }
    }
}