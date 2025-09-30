using Laba.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Laba.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class AnonymousOnlyFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        bool? isAuth = context.HttpContext.User?.Identity?.IsAuthenticated;

        if (isAuth == true)
            context.Result = new ForbidResult(authenticationScheme: JwtBearerDefaults.AuthenticationScheme);
    }
}