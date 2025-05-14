using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace PosBackend.Filters
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var requiredScopes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct();

            if (!requiredScopes.Any() && context.MethodInfo.DeclaringType != null)
            {
                // Check controller level attributes if method doesn't have any Authorize attributes
                requiredScopes = context.MethodInfo.DeclaringType
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Select(attr => attr.Policy)
                    .Distinct();
            }

            if (requiredScopes.Any())
            {
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                };

                operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };
            }
        }
    }
}
