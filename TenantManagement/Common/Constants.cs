using TenantManagement.Data.Entities;

namespace TenantManagement.Common.Constants
{
    public static class Constants
    {
        public const int MinPasswordLength = 12;
        public const string IncludeAll = $"{nameof(Roles)},{nameof(User.Accounts)}.{nameof(Account)},{nameof(User.Accounts)}.{nameof(Roles)},{nameof(User.Scopes)}";
    }
}