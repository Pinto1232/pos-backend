using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PosBackend.Filters
{
    public class OpenApiVersionFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Ensure the OpenAPI version is set to a valid value
            // This is a workaround for the "The provided definition does not specify a valid version field" error
            if (!swaggerDoc.Extensions.ContainsKey("swagger"))
            {
                swaggerDoc.Extensions.Add("swagger", new OpenApiString("2.0"));
            }
        }
    }
}