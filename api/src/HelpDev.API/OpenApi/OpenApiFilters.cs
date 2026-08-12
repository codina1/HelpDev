using HelpDev.API.Contracts;
using HelpDev.API.Security;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDev.API.OpenApi;

public sealed class OperationMetadataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = ApiDescriptionMetadataReader.Read(context.ApiDescription);

        if (!string.IsNullOrWhiteSpace(metadata.OperationId))
        {
            operation.OperationId = metadata.OperationId;
        }

        if (!string.IsNullOrWhiteSpace(metadata.Summary))
        {
            operation.Summary = metadata.Summary;
        }

        if (!string.IsNullOrWhiteSpace(metadata.Description))
        {
            operation.Description = metadata.Description;
        }

        if (metadata.Audience is not null)
        {
            operation.Extensions["x-helpdev-audience"] = new OpenApiString(metadata.Audience);
            operation.Extensions["x-helpdev-permission"] = new OpenApiString(MapPermission(metadata.Audience));
        }

        AnnotateRateLimitAndSize(operation, context);
        EnsureCorrelationHeaders(operation);
        ApplySecurity(operation, metadata.Audience);
        EnsureStandardErrorResponses(operation, context);
    }

    private static string MapPermission(string audience) =>
        audience switch
        {
            ApiAudiences.Admin => "admin-only",
            ApiAudiences.Authenticated => "authenticated-user",
            ApiAudiences.Operations => "operations",
            ApiAudiences.InternalCompatibility => "internal-compatibility",
            _ => "public",
        };

    private static void EnsureCorrelationHeaders(OpenApiOperation operation)
    {
        operation.Parameters ??= [];
        if (!operation.Parameters.Any(p =>
                string.Equals(p.Name, CorrelationIdMiddleware.HeaderName, StringComparison.OrdinalIgnoreCase)))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = CorrelationIdMiddleware.HeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description =
                    "Optional correlation identifier (max 100 characters; alphanumeric, hyphen, underscore, or dot). Invalid or missing values are replaced. Echoed on the response.",
                Schema = new OpenApiSchema { Type = "string", MaxLength = 100 },
            });
        }

        foreach (var response in operation.Responses.Values)
        {
            response.Headers ??= new Dictionary<string, OpenApiHeader>();
            response.Headers[CorrelationIdMiddleware.HeaderName] = new OpenApiHeader
            {
                Description = "Correlation identifier accepted or generated for this request.",
                Schema = new OpenApiSchema { Type = "string" },
            };
        }
    }

    private static void EnsureStandardErrorResponses(OpenApiOperation operation, OperationFilterContext context)
    {
        void Ensure(string statusCode, string description, OpenApiObject? example = null)
        {
            if (!operation.Responses.ContainsKey(statusCode))
            {
                operation.Responses[statusCode] = new OpenApiResponse { Description = description };
            }

            var response = operation.Responses[statusCode];
            response.Description ??= description;
            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ApiErrorResponse), context.SchemaRepository),
                    Example = example,
                };
            }
        }

        // Document common failures without inventing impossible codes per endpoint.
        if (operation.Responses.ContainsKey("400"))
        {
            Ensure("400", "Validation or malformed request.",
                OpenApiErrorExamples.Create("The request is invalid.", "validation_failed"));
        }

        if (operation.Responses.ContainsKey("401") || RequiresAuth(operation))
        {
            Ensure("401", "Authentication required or token invalid.",
                OpenApiErrorExamples.Create("Authentication is required.", "authentication_required"));
        }

        if (operation.Responses.ContainsKey("403") || IsAdmin(operation))
        {
            Ensure("403", "Authenticated but insufficient permission.",
                OpenApiErrorExamples.Create("You do not have permission to perform this action.", "access_denied"));
        }

        if (operation.Responses.ContainsKey("404"))
        {
            Ensure("404", "Resource not found.",
                OpenApiErrorExamples.Create("The requested resource was not found.", "resource_not_found"));
        }

        if (operation.Responses.ContainsKey("409"))
        {
            Ensure("409", "State or uniqueness conflict.",
                OpenApiErrorExamples.Create("The requested operation conflicts with the current state.", "resource_conflict"));
        }

        if (operation.Responses.ContainsKey("413"))
        {
            Ensure("413", "Request body exceeds configured limit.",
                OpenApiErrorExamples.Create("The request body is too large.", SecurityErrorCodes.RequestTooLarge));
            AddRetryAfterHeader(operation.Responses["413"]);
        }

        if (operation.Responses.ContainsKey("429"))
        {
            Ensure("429", "Rate limit exceeded.",
                OpenApiErrorExamples.Create("Too many requests. Try again later.", SecurityErrorCodes.RateLimitExceeded));
            AddRetryAfterHeader(operation.Responses["429"]);
        }

        if (operation.Responses.ContainsKey("503"))
        {
            Ensure("503", "Service not ready or critical dependency unavailable.",
                OpenApiErrorExamples.Create("The service is temporarily unavailable.", "service_unavailable"));
        }
    }

    private static void AddRetryAfterHeader(OpenApiResponse response)
    {
        response.Headers ??= new Dictionary<string, OpenApiHeader>();
        response.Headers["Retry-After"] = new OpenApiHeader
        {
            Description = "Estimated time before retrying, in seconds when provided.",
            Schema = new OpenApiSchema { Type = "string" },
        };
    }

    private static bool RequiresAuth(OpenApiOperation operation) =>
        operation.Extensions.TryGetValue("x-helpdev-audience", out var audience)
        && audience is OpenApiString s
        && (s.Value is ApiAudiences.Authenticated or ApiAudiences.Admin);

    private static bool IsAdmin(OpenApiOperation operation) =>
        operation.Extensions.TryGetValue("x-helpdev-audience", out var audience)
        && audience is OpenApiString s
        && s.Value == ApiAudiences.Admin;

    private static void ApplySecurity(OpenApiOperation operation, string? audience)
    {
        if (audience is ApiAudiences.Authenticated or ApiAudiences.Admin)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                },
            ];

            if (audience == ApiAudiences.Admin)
            {
                operation.Description = Append(
                    operation.Description,
                    "Requires an authenticated Admin role. Intended for trusted internal UI and operations.");
            }
        }
        else
        {
            operation.Security = [];
        }
    }

    private static void AnnotateRateLimitAndSize(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var notes = new List<string>();

        if (path.Contains("auth/send-otp", StringComparison.OrdinalIgnoreCase)
            || path.Contains("auth/verify-otp", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add("Request body limit: 16 KB. Dedicated OTP rate limits apply; 429 may be returned with Retry-After.");
            EnsureStatus(operation, "413");
            EnsureStatus(operation, "429");
        }
        else if (path.Contains("/execute", StringComparison.OrdinalIgnoreCase)
                 || path.Contains("/render", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add("Request body limit: 128 KB. Dedicated execution/render rate limits apply; 429 may be returned.");
            EnsureStatus(operation, "413");
            EnsureStatus(operation, "429");
        }
        else if (path.Contains("/search", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add("Search rate limits apply; 429 may be returned.");
            EnsureStatus(operation, "429");
        }
        else if (audienceIsAdmin(operation))
        {
            notes.Add("Admin endpoints are rate limited; oversized JSON bodies may return 413 (default JSON limit 256 KB).");
            EnsureStatus(operation, "413");
            EnsureStatus(operation, "429");
        }

        if (notes.Count > 0)
        {
            operation.Description = Append(operation.Description, string.Join(" ", notes));
        }
    }

    private static bool audienceIsAdmin(OpenApiOperation operation) =>
        operation.Extensions.TryGetValue("x-helpdev-audience", out var audience)
        && audience is OpenApiString s
        && s.Value == ApiAudiences.Admin;

    private static void EnsureStatus(OpenApiOperation operation, string status)
    {
        if (!operation.Responses.ContainsKey(status))
        {
            operation.Responses[status] = new OpenApiResponse { Description = status };
        }
    }

    private static string Append(string? existing, string addition) =>
        string.IsNullOrWhiteSpace(existing) ? addition : $"{existing.TrimEnd()} {addition}";
}

public sealed class ExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var operationId = operation.OperationId;
        if (string.IsNullOrWhiteSpace(operationId))
        {
            return;
        }

        if (operation.RequestBody?.Content.TryGetValue("application/json", out var requestMedia) == true)
        {
            requestMedia.Example = OpenApiExampleCatalog.GetRequestExample(operationId);
        }

        if (operation.Responses.TryGetValue("200", out var ok)
            && ok.Content is not null
            && ok.Content.TryGetValue("application/json", out var responseMedia))
        {
            var example = OpenApiExampleCatalog.GetResponseExample(operationId);
            if (example is not null)
            {
                responseMedia.Example = example;
            }
        }
    }
}

public sealed class AudienceDocumentFilter : IDocumentFilter
{
    private readonly string _documentName;

    public AudienceDocumentFilter(string documentName)
    {
        _documentName = documentName;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers =
        [
            new OpenApiServer { Url = "/", Description = "Relative to the API host" },
        ];

        OrderTags(swaggerDoc);
        AddHealthPaths(swaggerDoc);
    }

    private void OrderTags(OpenApiDocument swaggerDoc)
    {
        var order = new[]
        {
            ApiTags.Authentication,
            ApiTags.Profile,
            ApiTags.Content,
            ApiTags.Learning,
            ApiTags.Search,
            ApiTags.Toolbox,
            ApiTags.PromptLab,
            ApiTags.Administration,
            ApiTags.Analytics,
            ApiTags.Audit,
            ApiTags.Media,
            ApiTags.Operations,
            ApiTags.Outbox,
            ApiTags.Health,
        };

        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ApiTags.Authentication] = "OTP login and JWT issuance. Public endpoints; rate limited.",
            [ApiTags.Profile] = "Authenticated user profile operations.",
            [ApiTags.Content] = "Published content for anonymous consumers. Admin/writer mutations are authenticated.",
            [ApiTags.Learning] = "Public course catalog and authenticated enrollment/progress.",
            [ApiTags.Search] = "Published resource search. Indexing may be eventually consistent.",
            [ApiTags.Toolbox] = "Published tools catalog and execution.",
            [ApiTags.PromptLab] = "Published prompts catalog and rendering.",
            [ApiTags.Administration] = "Admin feature flags, settings, announcements, and user management.",
            [ApiTags.Analytics] = "Admin aggregate analytics reports. Eventually consistent.",
            [ApiTags.Audit] = "Admin immutable audit records with sanitized metadata.",
            [ApiTags.Media] = "Admin media library uploads and listing. Images only; no delete in v1.",
            [ApiTags.Operations] = "Admin operational status and health diagnostics.",
            [ApiTags.Outbox] = "Admin outbox recovery operations. Payloads are not exposed.",
            [ApiTags.Health] = "Process liveness and readiness probes. Anonymous.",
        };

        swaggerDoc.Tags = order
            .Where(tag => swaggerDoc.Paths.Values.SelectMany(p => p.Operations.Values)
                .Any(op => op.Tags.Any(t => string.Equals(t.Name, tag, StringComparison.OrdinalIgnoreCase)))
                || tag == ApiTags.Health)
            .Select(tag => new OpenApiTag
            {
                Name = tag,
                Description = descriptions.GetValueOrDefault(tag),
            })
            .ToList();
    }

    private void AddHealthPaths(OpenApiDocument swaggerDoc)
    {
        if (_documentName is not OpenApiDocumentNames.PublicV1
            and not OpenApiDocumentNames.AllV1
            and not OpenApiDocumentNames.AuthenticatedV1)
        {
            // Admin document still gets operations health via controllers; public health is useful in public/all.
        }

        if (_documentName is OpenApiDocumentNames.PublicV1 or OpenApiDocumentNames.AllV1)
        {
            AddMinimalHealth(swaggerDoc, "/health/live", "Operations_GetLiveness",
                "Process liveness probe. Does not check dependencies.", false);
            AddMinimalHealth(swaggerDoc, "/health/ready", "Operations_GetReadiness",
                "Dependency readiness probe. Returns 200 for Healthy/Degraded and 503 for Unhealthy.", false);
            AddLegacyHealth(swaggerDoc);
        }
    }

    private static void AddMinimalHealth(
        OpenApiDocument swaggerDoc,
        string path,
        string operationId,
        string description,
        bool deprecated)
    {
        if (swaggerDoc.Paths.ContainsKey(path))
        {
            return;
        }

        var statusSchema = new OpenApiSchema
        {
            Type = "object",
            Properties =
            {
                ["status"] = new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("Healthy"),
                },
            },
        };

        var operation = new OpenApiOperation
        {
            Tags = [new OpenApiTag { Name = ApiTags.Health }],
            OperationId = operationId,
            Summary = path.Contains("live", StringComparison.Ordinal) ? "Liveness probe" : "Readiness probe",
            Description = description,
            Deprecated = deprecated,
            Security = [],
            Extensions =
            {
                ["x-helpdev-audience"] = new OpenApiString(ApiAudiences.Operations),
                ["x-helpdev-permission"] = new OpenApiString("public"),
            },
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "Process or dependency status.",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = statusSchema,
                            Example = new OpenApiObject
                            {
                                ["status"] = new OpenApiString("Healthy"),
                            },
                        },
                    },
                },
            },
        };

        if (path.Contains("ready", StringComparison.Ordinal))
        {
            operation.Responses["503"] = new OpenApiResponse
            {
                Description = "Service not ready.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = statusSchema,
                        Example = new OpenApiObject
                        {
                            ["status"] = new OpenApiString("Unhealthy"),
                        },
                    },
                },
            };
        }

        swaggerDoc.Paths.Add(path, new OpenApiPathItem
        {
            Operations = { [OperationType.Get] = operation },
        });
    }

    private static void AddLegacyHealth(OpenApiDocument swaggerDoc)
    {
        const string path = "/api/health";
        if (swaggerDoc.Paths.ContainsKey(path))
        {
            return;
        }

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties =
            {
                ["status"] = new OpenApiSchema { Type = "string" },
                ["service"] = new OpenApiSchema { Type = "string" },
                ["database"] = new OpenApiSchema
                {
                    Type = "object",
                    Properties =
                    {
                        ["provider"] = new OpenApiSchema { Type = "string" },
                        ["connected"] = new OpenApiSchema { Type = "boolean" },
                    },
                },
            },
        };

        swaggerDoc.Paths.Add(path, new OpenApiPathItem
        {
            Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = [new OpenApiTag { Name = ApiTags.Health }],
                    OperationId = "Operations_LegacyHealth",
                    Summary = "Legacy health endpoint",
                    Description =
                        "Legacy health endpoint retained for backward compatibility. New integrations should use `/health/live` and `/health/ready`.",
                    Deprecated = true,
                    Security = [],
                    Extensions =
                    {
                        ["x-helpdev-audience"] = new OpenApiString(ApiAudiences.InternalCompatibility),
                        ["x-helpdev-permission"] = new OpenApiString("public"),
                    },
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Database reachable.",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = schema,
                                    Example = new OpenApiObject
                                    {
                                        ["status"] = new OpenApiString("Healthy"),
                                        ["service"] = new OpenApiString("HelpDev API"),
                                        ["database"] = new OpenApiObject
                                        {
                                            ["provider"] = new OpenApiString("PostgreSQL"),
                                            ["connected"] = new OpenApiBoolean(true),
                                        },
                                    },
                                },
                            },
                        },
                        ["503"] = new OpenApiResponse
                        {
                            Description = "Database unavailable (Degraded payload).",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType { Schema = schema },
                            },
                        },
                    },
                },
            },
        });
    }
}

public sealed class SchemaDescriptionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        Describe(swaggerDoc, "ApiErrorResponse",
            "Canonical API error response with a human-readable message and stable code. Correlation is returned via the X-Correlation-ID response header.");
        Describe(swaggerDoc, "AuthResponse",
            "Successful OTP verification result containing an access token placeholder and authenticated user profile fields. Timestamps are UTC.");
        Describe(swaggerDoc, "SendOtpResponse",
            "OTP request acknowledgement. OTP values are never returned in Production.");
        Describe(swaggerDoc, "SendOtpRequest",
            "Request body for sending a login OTP to a mobile number.");
        Describe(swaggerDoc, "VerifyOtpRequest",
            "Request body for verifying a login OTP and issuing a JWT.");
    }

    private static void Describe(OpenApiDocument doc, string schemaName, string description)
    {
        if (doc.Components?.Schemas is null)
        {
            return;
        }

        if (doc.Components.Schemas.TryGetValue(schemaName, out var schema))
        {
            schema.Description = description;
        }
    }
}
