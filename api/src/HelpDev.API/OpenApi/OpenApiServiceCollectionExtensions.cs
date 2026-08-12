using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDev.API.OpenApi;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddHelpDevApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    public static IServiceCollection AddHelpDevOpenApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<OpenApiOptions>, OpenApiOptionsValidator>();
        services.AddOptions<OpenApiOptions>()
            .Bind(configuration.GetSection(OpenApiOptions.SectionName))
            .ValidateOnStart();

        var openApiOptions = configuration.GetSection(OpenApiOptions.SectionName).Get<OpenApiOptions>()
            ?? new OpenApiOptions();

        if (!ShouldRegisterOpenApi(openApiOptions, environment))
        {
            return services;
        }

        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(SchemaIdSelector.GetSchemaId);

            if (openApiOptions.ExposePublicDocument)
            {
                options.SwaggerDoc(OpenApiDocumentNames.PublicV1, CreateInfo(
                    "HelpDev Public API",
                    "Public anonymous endpoints for HelpDev API v1."));
            }

            if (openApiOptions.ExposeAuthenticatedDocument)
            {
                options.SwaggerDoc(OpenApiDocumentNames.AuthenticatedV1, CreateInfo(
                    "HelpDev Authenticated API",
                    "Authenticated user endpoints for HelpDev API v1. Requires Bearer JWT."));
            }

            if (openApiOptions.ExposeAdminDocument)
            {
                options.SwaggerDoc(OpenApiDocumentNames.AdminV1, CreateInfo(
                    "HelpDev Admin API",
                    "Admin-only endpoints for trusted internal UI and operations. Requires Admin role."));
            }

            if (openApiOptions.ExposeCompleteDocument)
            {
                options.SwaggerDoc(OpenApiDocumentNames.AllV1, CreateInfo(
                    "HelpDev Complete API",
                    "Complete HelpDev API v1 surface for internal consumers."));
            }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT obtained after OTP verification. Send as: Bearer {token}. Tokens expire. Admin endpoints require the Admin role. Do not paste real tokens into shared documentation.",
            });

            // Do not add a global security requirement; operation filter applies Bearer only where needed.
            options.DocInclusionPredicate((documentName, apiDescription) =>
                IncludeInDocument(documentName, apiDescription));

            options.OperationFilter<OperationMetadataOperationFilter>();
            options.OperationFilter<ExampleOperationFilter>();
            options.DocumentFilter<SchemaDescriptionDocumentFilter>();
            options.DocumentFilter<AudienceDocumentFilterResolver>();

            options.TagActionsBy(api =>
            {
                var tags = api.ActionDescriptor.EndpointMetadata
                    .OfType<TagsAttribute>()
                    .SelectMany(t => t.Tags)
                    .ToArray();
                if (tags.Length > 0)
                {
                    return tags;
                }

                if (api.GroupName is not null)
                {
                    return [api.GroupName];
                }

                return ["Untagged"];
            });

            options.OrderActionsBy(api => api.RelativePath ?? string.Empty);

            var xmlPath = Path.Combine(AppContext.BaseDirectory, "HelpDev.API.xml");
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });

        return services;
    }

    public static bool ShouldExposeOpenApi(OpenApiOptions options, IHostEnvironment environment)
    {
        if (!options.Enabled)
        {
            return false;
        }

        if (environment.IsProduction() && !options.EnableInProduction)
        {
            return false;
        }

        return true;
    }

    private static bool ShouldRegisterOpenApi(OpenApiOptions options, IHostEnvironment environment) =>
        ShouldExposeOpenApi(options, environment) || environment.IsEnvironment("Testing");

    private static OpenApiInfo CreateInfo(string title, string description) =>
        new()
        {
            Title = title,
            Version = "v1",
            Description = description +
                " Existing unversioned `/api/...` routes remain supported as v1 compatibility aliases." +
                " Canonical documented routes use `/api/v1/...`. Timestamps are UTC (ISO 8601)." +
                " Errors use `{ message, code }`. Correlation via `X-Correlation-ID`.",
        };

    private static bool IncludeInDocument(
        string documentName,
        Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>()
            .Any(x => x.IgnoreApi))
        {
            return false;
        }

        // Prefer canonical versioned routes to avoid duplicate OperationIds.
        if (!OpenApiPathHelpers.IsCanonicalVersionedApiPath(apiDescription.RelativePath))
        {
            return false;
        }

        var audience = ResolveAudience(apiDescription);

        return documentName switch
        {
            OpenApiDocumentNames.PublicV1 =>
                audience is ApiAudiences.Public or ApiAudiences.Operations or ApiAudiences.InternalCompatibility,
            OpenApiDocumentNames.AuthenticatedV1 =>
                audience is ApiAudiences.Authenticated,
            OpenApiDocumentNames.AdminV1 =>
                audience is ApiAudiences.Admin,
            OpenApiDocumentNames.AllV1 => true,
            _ => false,
        };
    }

    private static string ResolveAudience(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerAction)
        {
            var methodAudience = controllerAction.MethodInfo.GetCustomAttribute<ApiAudienceAttribute>(inherit: true);
            if (methodAudience is not null)
            {
                return methodAudience.Audience;
            }

            var controllerAudience = controllerAction.ControllerTypeInfo.GetCustomAttribute<ApiAudienceAttribute>(inherit: true);
            if (controllerAudience is not null)
            {
                return controllerAudience.Audience;
            }
        }

        return ApiAudiences.Public;
    }
}

/// <summary>
/// Resolves the active document name for AudienceDocumentFilter.
/// </summary>
public sealed class AudienceDocumentFilterResolver : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        new AudienceDocumentFilter(context.DocumentName).Apply(swaggerDoc, context);
    }
}

public static class OpenApiApplicationBuilderExtensions
{
    public static WebApplication UseHelpDevOpenApi(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value;
        if (!OpenApiServiceCollectionExtensions.ShouldExposeOpenApi(options, app.Environment))
        {
            return app;
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "openapi/{documentName}.json";
        });

        if (options.EnableUi)
        {
            app.UseSwaggerUI(c =>
            {
                if (options.ExposePublicDocument)
                {
                    c.SwaggerEndpoint($"/openapi/{OpenApiDocumentNames.PublicV1}.json", "Public API v1");
                }

                if (options.ExposeAuthenticatedDocument)
                {
                    c.SwaggerEndpoint($"/openapi/{OpenApiDocumentNames.AuthenticatedV1}.json", "Authenticated API v1");
                }

                if (options.ExposeAdminDocument)
                {
                    c.SwaggerEndpoint($"/openapi/{OpenApiDocumentNames.AdminV1}.json", "Admin API v1");
                }

                if (options.ExposeCompleteDocument)
                {
                    c.SwaggerEndpoint($"/openapi/{OpenApiDocumentNames.AllV1}.json", "Complete API v1");
                }

                c.EnableDeepLinking();
                c.DisplayOperationId();
                c.DefaultModelsExpandDepth(1);
                // PersistAuthorization disabled by default for shared environments.
            });
        }

        return app;
    }
}

public static class OpenApiExport
{
    public static async Task<int> ExportAsync(WebApplication app, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);
        var swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();
        var options = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value;

        var documents = new List<(string Name, string FileName)>();
        if (options.ExposePublicDocument)
        {
            documents.Add((OpenApiDocumentNames.PublicV1, "helpdev-public-v1.json"));
        }

        if (options.ExposeAuthenticatedDocument)
        {
            documents.Add((OpenApiDocumentNames.AuthenticatedV1, "helpdev-authenticated-v1.json"));
        }

        if (options.ExposeAdminDocument)
        {
            documents.Add((OpenApiDocumentNames.AdminV1, "helpdev-admin-v1.json"));
        }

        if (options.ExposeCompleteDocument)
        {
            documents.Add((OpenApiDocumentNames.AllV1, "helpdev-all-v1.json"));
        }

        foreach (var (name, fileName) in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = swaggerProvider.GetSwagger(name);
            var path = Path.Combine(outputDirectory, fileName);
            await using var stream = File.Create(path);
            await using var streamWriter = new StreamWriter(stream);
            var writer = new Microsoft.OpenApi.Writers.OpenApiJsonWriter(streamWriter);
            document.SerializeAsV3(writer);
            await streamWriter.FlushAsync(cancellationToken);
        }

        return 0;
    }
}
