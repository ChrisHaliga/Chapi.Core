using Chapi.Api.Models;
using Chapi.Api.Services.ApiServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chapi.Api.Middleware
{
    public class CustomAuthorization : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _eligiblePermissions;
        private const string ApplicationName = "Chapi";

        public CustomAuthorization(string[] eligiblePermissions) => _eligiblePermissions = eligiblePermissions;


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var usersService = context.HttpContext.RequestServices.GetRequiredService<UserApiService>();

            var email = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type.EndsWith("emailaddress"));

            if (!string.IsNullOrEmpty(email?.Value) && (context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                var user = await usersService.GetItem(new UserWithId(email.Value), context.HttpContext.RequestAborted);

                if (user != null)
                {
                    var thisApplication = user.Applications.Find(x => x.Name == ApplicationName);
                    if (thisApplication != null)
                    {
                        foreach (var permission in _eligiblePermissions)
                        {
                            if (thisApplication.Roles.Contains(permission))
                            {
                                return;
                            }
                        }
                    }
                }
            }

            context.Result = new UnauthorizedResult();
        }
    }
}
