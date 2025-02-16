using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chapi.Api.Middleware
{
    public class ApiKeyAuthorization : IAuthorizationFilter
    {
        private readonly string _authorizationKey;
        public static readonly string Header = "X-Api-Key";

        public ApiKeyAuthorization(string authorizationKey) => _authorizationKey = authorizationKey;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(!context.HttpContext.Request.Headers.TryGetValue(Header, out var apiKey) || apiKey != _authorizationKey)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

    public class ApiKeyAuthorizationAttribute : ServiceFilterAttribute
    {
        public ApiKeyAuthorizationAttribute(): base(typeof(ApiKeyAuthorization)) {}
    }

    public class ApiKeyAuthorizationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasApiKeyFilter = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<ServiceFilterAttribute>()
                .Any(a => a.ServiceType == typeof(ApiKeyAuthorization))
                ||
                (context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<ServiceFilterAttribute>()
                .Any(a => a.ServiceType == typeof(ApiKeyAuthorization)) ?? false);

            if (hasApiKeyFilter)
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>();
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = ApiKeyAuthorization.Header,
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
