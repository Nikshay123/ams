namespace TenantManagement.Common
{
    public static class AppGlobals
    {
        public static string BASE_DOMAIN_CONFIG_NAME = "BaseDomain";
        public static string BASE_API_DOMAIN_CONFIG_NAME = "BaseApiDomain";
        public static string ClaimTypeSubjectId = "sub";
        public static string ClaimTypeTenantId = "tid";
        public static string ClaimTypeOrgId = "org";
        public static string ClaimTypeScopeId = "scope";
        public static string ClaimTypeTenantIdUri = "http://schemas.microsoft.com/identity/claims/tenantid";
        public static string ClaimTypeTestScope = "http://itt/claims/scopes/test";
        public static int RefreshTokenExpiryDays = 7;
        public static int TransientAuthExpiryMinutes = 15;
        public static int TransientAuthExpiryDays = 3;
    }
}