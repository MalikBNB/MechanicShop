using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MechanicShop.Api.OpenApi.Transformers;

/// <summary>
/// This class tells Swagger how auth works + Mark endpoints that require auth.
/// IOpenApiDocumentTransformer --> When it runs: Once for the whole Swagger document --> Purpose: Defines the JWT security scheme
/// IOpenApiOperationTransformer --> When it runs: For each endpoint --> Purpose: Adds security requirement if [Authorize] exists
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;

    // This part adds a Bearer security scheme to Swagger.
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Ensures Swagger has a place to store security schemes.
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        /*
        This tells Swagger:

        ✔ Auth type = HTTP Bearer
        ✔ Token format = JWT
        ✔ Header name = Authorization
        ✔ Usage = Authorization: Bearer {token}

        So in Swagger UI you’ll see the Authorize 🔒 button.
        */
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Bearer token",
            Name = "Authorization",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = SchemeId
            }
        };

        return Task.CompletedTask;
    }

    // Protect endpoints automatically. This runs for each API endpoint.
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // This checks if the endpoint has: [Authorize] --> If yes → Swagger marks it as secured.
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
        {
            // Ensure the endpoint has a security list.
            operation.Security ??= [];

            // This references the scheme defined earlier (Bearer JWT).
            var key = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference()
            };

            key.Reference.Type = ReferenceType.SecurityScheme;
            key.Reference.Id = SchemeId;

            // This tells Swagger: This endpoint requires Bearer token.
            var requirement = new OpenApiSecurityRequirement
            {
                { key, [] },
            };

            operation.Security.Add(requirement);
        }

        return Task.CompletedTask;
    }
}