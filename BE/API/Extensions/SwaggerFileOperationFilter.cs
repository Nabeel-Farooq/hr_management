using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace API.Extensions;

public sealed class SwaggerFileOperationFilter : IOperationFilter
{
    private const string MultipartFormData = "multipart/form-data";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content is null ||
            !operation.RequestBody.Content.TryGetValue(MultipartFormData, out var mediaType))
        {
            return;
        }

        var fileParameters = context.MethodInfo
            .GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToArray();

        if (fileParameters.Length == 0)
            return;

        mediaType.Schema ??= new OpenApiSchema();
        mediaType.Schema.Properties ??= new Dictionary<string, OpenApiSchema>();

        foreach (var param in fileParameters)
        {
            mediaType.Schema.Properties[param.Name ?? "file"] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
        }
    }
}
