using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace TenantManagement.Common
{
    //****NOTE****
    //Roles should be order from most privilege to least privilege
    public enum Roles
    {
        None,
        AppAdmin,

        //User Only Roles
        AnyUserRole,

        AnyManageRole,

        Admin,
        Manager,
        User,

        //Account Roles
        AnyAccountRole,

        AnyAccountManageRole,

        AccountOwner,
        AccountAdmin,
        AccountManager,
        AccountUser,
        AccountStakeholder
    }

    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params Roles[] roles)
        {
            const string anyPrefix = "Any";
            List<string> roleList = new();

            foreach (var role in roles)
            {
                if (role.ToString().StartsWith(anyPrefix))
                {
                    switch (role)
                    {
                        case Common.Roles.AnyUserRole:
                            roleList.Add(Common.Roles.User.ToString());
                            goto case Common.Roles.AnyManageRole;
                        case Common.Roles.AnyManageRole:
                            roleList.Add(Common.Roles.Admin.ToString());
                            roleList.Add(Common.Roles.Manager.ToString());
                            break;

                        case Common.Roles.AnyAccountRole:
                            roleList.Add(Common.Roles.AccountUser.ToString());
                            roleList.Add(Common.Roles.AccountStakeholder.ToString());
                            goto case Common.Roles.AnyAccountManageRole;
                        case Common.Roles.AnyAccountManageRole:
                            roleList.Add(Common.Roles.AccountOwner.ToString());
                            roleList.Add(Common.Roles.AccountAdmin.ToString());
                            roleList.Add(Common.Roles.AccountManager.ToString());
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    roleList.Add(role.ToString());
                }
            }

            Roles = string.Join(',', roleList);
        }
    }
}