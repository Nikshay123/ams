using System.Collections.Generic;

namespace TenantManagement.Common
{
    //Templates Values must match the name of the Razor page to be rendered
    public enum EmailTemplates
    {
        None,
        Invitation,
        Verification,
        ChangeEmailNotification,
        ChangeEmailPasswordReset,
        PasswordReset,
        AccountAccess,
        AccountDeactivated
    }

    public enum EmailDataKeys
    {
        Domain,
        Name,
        Id,
        Code,
        FromName,
        FromEmail,
        AccountName
    }
}