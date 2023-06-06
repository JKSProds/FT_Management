using Microsoft.AspNetCore.Authorization;
using FT_Management.BasicAuthentication.Shared.Authentication.Basic;

namespace FT_Management.BasicAuthentication.Authentication.Basic.Attributes
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme;
        }
    }
}

