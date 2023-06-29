using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TenantManagement.Common
{
    public class EmailConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey,
            RouteValueDictionary values, RouteDirection routeDirection)
        {
            string email = values[routeKey]?.ToString();
            if (email != null && new EmailAddressAttribute().IsValid(email))
            {
                return true;
            }
            return false;
        }
    }
}
